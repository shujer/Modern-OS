#include "Thunder.h"
#include <algorithm>

USING_NS_CC;

using namespace CocosDenshion;

Scene* Thunder::createScene() {
  // 'scene' is an autorelease object
  auto scene = Scene::create();

  // 'layer' is an autorelease object
  auto layer = Thunder::create();

  // add layer as a child to scene
  scene->addChild(layer);

  // return the scene
  return scene;
}

bool Thunder::init() {
  if (!Layer::init()) {
    return false;
  }

  isMove = false;  // 是否点击飞船
  visibleSize = Director::getInstance()->getVisibleSize();


  // 创建背景
  auto bgsprite = Sprite::create("bg.jpg");
  bgsprite->setPosition(visibleSize / 2);
  bgsprite->setScale(visibleSize.width / bgsprite->getContentSize().width,
                     visibleSize.height / bgsprite->getContentSize().height);
  this->addChild(bgsprite, 0);

  // 创建飞船
  player = Sprite::create("player.png");
  player->setAnchorPoint(Vec2(0.5, 0.5));
  player->setPosition(visibleSize.width / 2, player->getContentSize().height);
  player->setName("player");
  this->addChild(player, 1);

  // 显示陨石和子弹数量
  enemysNum = Label::createWithTTF("enemys: 0", "fonts/arial.TTF", 20);
  enemysNum->setColor(Color3B(255, 255, 255));
  enemysNum->setPosition(50, 60);
  this->addChild(enemysNum, 3);
  bulletsNum = Label::createWithTTF("bullets: 0", "fonts/arial.TTF", 20);
  bulletsNum->setColor(Color3B(255, 255, 255));
  bulletsNum->setPosition(50, 30);
  this->addChild(bulletsNum, 3);

  addEnemy(5);        // 初始化陨石
  preloadMusic();     // 预加载音乐
  playBgm();          // 播放背景音乐
  explosion();        // 创建爆炸帧动画

  // 添加监听器
  addTouchListener();
  addKeyboardListener();
  addCustomListener();

  // 调度器
  schedule(schedule_selector(Thunder::update), 0.04f, kRepeatForever, 0);
  return true;
}

//预加载音乐文件
void Thunder::preloadMusic() {
	auto audio = SimpleAudioEngine::getInstance();
	audio->preloadBackgroundMusic("music/bgm.mp3");
	audio->preloadEffect("music/fire.wav");
	audio->preloadEffect("music/explore.wav");

}

//播放背景音乐
void Thunder::playBgm() {
	SimpleAudioEngine::getInstance()->playBackgroundMusic("music/bgm.mp3");
}

//初始化陨石
void Thunder::addEnemy(int n) {
	number_y = 3;
  enemys.clear();
  for (unsigned i = 0; i < 3; ++i) {
    char enemyPath[20];
    sprintf(enemyPath, "stone%d.png", 3 - i);
    double width = visibleSize.width / (n + 1.0),
           height = visibleSize.height - (50 * (i + 1));
    for (int j = 0; j < n; ++j) {
      auto enemy = Sprite::create(enemyPath);
      enemy->setAnchorPoint(Vec2(0.5, 0.5));
      enemy->setScale(0.5, 0.5);
      enemy->setPosition(width * (j + 1), height);
      enemys.push_back(enemy);
      addChild(enemy, 1);
    }
  }
}

// 陨石向下移动并生成新的一行(加分项)
void Thunder::newEnemy() {
	for (auto enemy : enemys) {
		enemy->setPosition(enemy->getPosition() + Vec2(0,-50));
	}
	int index = number_y % 3 + 1;
		char enemyPath[20];
		sprintf(enemyPath, "stone%d.png", index);
		double width = visibleSize.width / (5 + 1.0);
		for (int j = 0; j < 5; ++j) {
			auto enemy = Sprite::create(enemyPath);
			enemy->setAnchorPoint(Vec2(0.5, 0.5));
			enemy->setScale(0.5, 0.5);
			enemy->setPosition(width * (j+1) - 80, visibleSize.height -50);
			enemys.push_back(enemy);
			addChild(enemy, 1);
	}
		number_y++;
}

// 移动飞船
void Thunder::movePlane(char c) {
	float offset_x = 0;
	Vec2 pos;
	switch (c)
	{
	case 'A':
		offset_x = player->getPosition().x <= 30  ? 0 : -30;
		break;
	case 'D':
		offset_x = player->getPosition().x >= visibleSize.width - 30 ? 0 : 30; 
		break;
	default:
		break;
	}
	player->runAction(MoveBy::create(1.0f,Point(offset_x,0)));
}

//发射子弹
void Thunder::fire() {
  auto bullet = Sprite::create("bullet.png");
  bullet->setAnchorPoint(Vec2(0.5, 0.5));
  bullets.push_back(bullet);
  bullet->setPosition(player->getPosition());
  addChild(bullet, 1);
  SimpleAudioEngine::getInstance()->playEffect("music/fire.wav", false);

  // 移除飞出屏幕外的子弹
  for (auto itb = bullets.begin(); itb != bullets.end();) {
	  auto b = *itb;
	  if (b->getPosition().y > visibleSize.height) {
		  itb = bullets.erase(itb);
	  }
	  else
	  {
		  itb++;
	  }
  }
  bullet->runAction(MoveBy::create(1.0f, Vec2(0, visibleSize.height)));

}

// 切割爆炸动画帧
void Thunder::explosion() {
	auto texture3 = Director::getInstance()->getTextureCache()->addImage("explosion.png");
	for (int i = 0; i < 8; i++) {
		auto frame = SpriteFrame::createWithTexture(texture3, CC_RECT_PIXELS_TO_POINTS(Rect(188 * i, 0, 188, 212)));
		explore.pushBack(frame);
	}
}

void Thunder::update(float f) {
  // 实时更新页面内陨石和子弹数量(不得删除)
  // 要求数量显示正确(加分项)
  char str[15];
  sprintf(str, "enemys: %d", enemys.size());
  enemysNum->setString(str);
  sprintf(str, "bullets: %d", bullets.size());
  bulletsNum->setString(str);

  // 飞船移动
  if (isMove)
    this->movePlane(movekey);

  static int ct = 0;
  static int dir = 4;
  ++ct;
  if (ct == 120)
    ct = 40, dir = -dir;
  else if (ct == 80) {
    dir = -dir;
    newEnemy();  // 陨石向下移动并生成新的一行(加分项)
  }
  else if (ct == 20)
    ct = 40, dir = -dir;

  //陨石左右移动
  for (Sprite* s : enemys) {
    if (s != NULL) {
      s->setPosition(s->getPosition() + Vec2(dir, 0));
    }
  }

  // 分发自定义事件
  EventCustom e("meet");
  _eventDispatcher->dispatchEvent(&e);
}

// 自定义碰撞事件
void Thunder::meet(EventCustom * event) {
  // 判断子弹是否打中陨石并执行对应操作
	for (auto it1 = bullets.begin(); it1 != bullets.end(); it1++) {
		for (auto it2 = enemys.begin(); it2 != enemys.end();) {
			if ((*it1)->getPosition().getDistance((*it2)->getPosition())<25) {
				auto enemy = *it2;
				Sprite* tmp = enemy;
				enemy->runAction(
					Sequence::create(
						Animate::create(
							Animation::createWithSpriteFrames(explore, 0.05f, 1)
						),
						CallFunc::create([=] {
							tmp->removeFromParentAndCleanup(true);						
							}
						),
						nullptr
					)
				);
				SimpleAudioEngine::getInstance()->playEffect("music/explore.wav", false);
				it2 = enemys.erase(it2);
				return;
			}
			else{
				it2++;
			}
		}
	}

  // 判断游戏是否结束并执行对应操作
	for (auto enemy : enemys) {
		if (enemy->getPosition().y <= player->getPosition().y + 70) {
			auto l = Sprite::create("gameOver.png");
			l->setPosition(visibleSize.width / 2, visibleSize.height / 2);
			addChild(l,3);
			Director::getInstance()->stopAnimation();
			_eventDispatcher->removeAllEventListeners();
		}
  }
}

// 添加自定义监听器
void Thunder::addCustomListener() {
	auto meetListener = EventListenerCustom::create("meet", CC_CALLBACK_1(Thunder::meet, this));
	_eventDispatcher->addEventListenerWithFixedPriority(meetListener, 1);
}

// 添加键盘事件监听器
void Thunder::addKeyboardListener() {
	auto keylistener = EventListenerKeyboard::create();
	keylistener->onKeyPressed = CC_CALLBACK_2(Thunder::onKeyPressed, this);
	keylistener->onKeyReleased = CC_CALLBACK_2(Thunder::onKeyReleased, this);
	this->getEventDispatcher()->addEventListenerWithSceneGraphPriority(keylistener, this);
  
}

void Thunder::onKeyPressed(EventKeyboard::KeyCode code, Event* event) {
  switch (code) {
  case EventKeyboard::KeyCode::KEY_LEFT_ARROW:
  case EventKeyboard::KeyCode::KEY_CAPITAL_A:
  case EventKeyboard::KeyCode::KEY_A:
    movekey = 'A';
    isMove = true;
    break;
  case EventKeyboard::KeyCode::KEY_RIGHT_ARROW:
  case EventKeyboard::KeyCode::KEY_CAPITAL_D:
  case EventKeyboard::KeyCode::KEY_D:
    movekey = 'D';
    isMove = true;
    break;
  case EventKeyboard::KeyCode::KEY_SPACE:
    fire();
    break;
  }
}

void Thunder::onKeyReleased(EventKeyboard::KeyCode code, Event* event) {
  switch (code) {
  case EventKeyboard::KeyCode::KEY_LEFT_ARROW:
  case EventKeyboard::KeyCode::KEY_A:
  case EventKeyboard::KeyCode::KEY_CAPITAL_A:
  case EventKeyboard::KeyCode::KEY_RIGHT_ARROW:
  case EventKeyboard::KeyCode::KEY_D:
  case EventKeyboard::KeyCode::KEY_CAPITAL_D:
    isMove = false;
    break;
  }
}


// 添加触摸事件监听器
void Thunder::addTouchListener() {
	auto touchlistener = EventListenerTouchOneByOne::create();
	touchlistener->onTouchMoved = CC_CALLBACK_2(Thunder::onTouchMoved, this);
	touchlistener->onTouchBegan = CC_CALLBACK_2(Thunder::onTouchBegan, this);
	touchlistener->onTouchEnded = CC_CALLBACK_2(Thunder::onTouchEnded, this);
	this->getEventDispatcher()->addEventListenerWithSceneGraphPriority(touchlistener, this);
}

// 鼠标点击发射炮弹
bool Thunder::onTouchBegan(Touch *touch, Event *event) {
	fire();
  return true;
}

void Thunder::onTouchEnded(Touch *touch, Event *event) {
  isClick = false;
}

// 当鼠标按住飞船后可控制飞船移动 (加分项)
void Thunder::onTouchMoved(Touch *touch, Event *event) {
	Vec2 delta = touch->getDelta();
	player->setPosition(player->getPosition() + Vec2(delta.x, 0));
}
