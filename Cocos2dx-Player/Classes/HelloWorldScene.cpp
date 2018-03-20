#include "HelloWorldScene.h"
#include "Monster.h"
#pragma execution_character_set("utf-8")
USING_NS_CC;

Scene* HelloWorld::createScene()
{
    // 'scene' is an autorelease object
    auto scene = Scene::create();

    // 'layer' is an autorelease object
    auto layer = HelloWorld::create();

    // add layer as a child to scene
    scene->addChild(layer);

    // return the scene
    return scene;
}

// on "init" you need to initialize your instance
bool HelloWorld::init()
{
    //////////////////////////////
    // 1. super init first
    if ( !Layer::init() )
    {
        return false;
    }
	visibleSize = Director::getInstance()->getVisibleSize();
    origin = Director::getInstance()->getVisibleOrigin();

	TMXTiledMap* tmx = TMXTiledMap::create("map.tmx");
	tmx->setPosition(visibleSize.width / 2, visibleSize.height / 2); tmx->setAnchorPoint(Vec2(0.5, 0.5));
	tmx->setScale(Director::getInstance()->getContentScaleFactor());
	this->addChild(tmx, 0);


	schedule(schedule_selector(HelloWorld::updateCustom), 4.0f, kRepeatForever, 0);


	dtime = 180;
	schedule(schedule_selector(HelloWorld::update), 1.0f, kRepeatForever,1);
	perc = 100;

    
	//时间
	time = Label::create("180", "fonts/arial.ttf", 36);
	time->setPosition(330,visibleSize.height-20);
	addChild(time, 3);

	//创建文字label
	auto ylabel = Label::createWithTTF("Y", "fonts/arial.ttf", 36);
	auto wlabel = Label::createWithTTF("W", "fonts/arial.ttf", 36);
	auto alabel = Label::createWithTTF("A", "fonts/arial.ttf", 36);
	auto slabel = Label::createWithTTF("S", "fonts/arial.ttf", 36);
	auto dlabel = Label::createWithTTF("D", "fonts/arial.ttf", 36);

	auto yItem = MenuItemLabel::create(ylabel, CC_CALLBACK_1(HelloWorld::yClickCallback, this));
	yItem->setPosition(Vec2(visibleSize.width - 72, 36));
	auto ymenu = Menu::create(yItem, NULL);
	ymenu->setPosition(Vec2::ZERO);
	this->addChild(ymenu, 1);

	auto wItem = MenuItemLabel::create(wlabel, CC_CALLBACK_1(HelloWorld::wClickCallback, this));
	wItem->setPosition(Vec2(72, 72));
	auto wmenu = Menu::create(wItem, NULL);
	wmenu->setPosition(Vec2::ZERO);
	this->addChild(wmenu, 1);

	auto aItem = MenuItemLabel::create(alabel, CC_CALLBACK_1(HelloWorld::aClickCallback, this));
	aItem->setPosition(Vec2(36, 36));
	auto amenu = Menu::create(aItem, NULL);
	amenu->setPosition(Vec2::ZERO);
	this->addChild(amenu, 1);

	auto sItem = MenuItemLabel::create(slabel, CC_CALLBACK_1(HelloWorld::sClickCallback, this));
	sItem->setPosition(Vec2(72,36));
	auto smenu = Menu::create(sItem, NULL);
	smenu->setPosition(Vec2::ZERO);
	this->addChild(smenu, 1);

	auto dItem = MenuItemLabel::create(dlabel, CC_CALLBACK_1(HelloWorld::dClickCallback, this));
	dItem->setPosition(Vec2(108,36));
	auto dmenu = Menu::create(dItem, NULL);
	dmenu->setPosition(Vec2::ZERO);
	this->addChild(dmenu, 1);


	//创建一张贴图
	auto texture = Director::getInstance()->getTextureCache()->addImage("$lucia_2.png");
	//从贴图中以像素单位切割，创建关键帧
	auto frame0 = SpriteFrame::createWithTexture(texture, CC_RECT_PIXELS_TO_POINTS(Rect(0, 0, 113, 113)));
	//使用第一帧创建精灵
	player = Sprite::createWithSpriteFrame(frame0);
	player->setPosition(Vec2(origin.x + visibleSize.width / 2,
							origin.y + visibleSize.height/2));
	addChild(player, 3);

	//hp条
	Sprite* sp0 = Sprite::create("hp.png", CC_RECT_PIXELS_TO_POINTS(Rect(0, 320, 420, 47)));
	Sprite* sp = Sprite::create("hp.png", CC_RECT_PIXELS_TO_POINTS(Rect(610, 362, 4, 16)));

	//使用hp条设置progressBar
	pT = ProgressTimer::create(sp);
	pT->setScaleX(90);
	pT->setAnchorPoint(Vec2(0, 0));
	pT->setType(ProgressTimerType::BAR);
	pT->setBarChangeRate(Point(1, 0));
	pT->setMidpoint(Point(0, 1));
	pT->setPercentage(100);
	pT->setPosition(Vec2(origin.x+14*pT->getContentSize().width,origin.y + visibleSize.height - 2*pT->getContentSize().height));
	addChild(pT,1);
	sp0->setAnchorPoint(Vec2(0, 0));
	sp0->setPosition(Vec2(origin.x + pT->getContentSize().width, origin.y + visibleSize.height - sp0->getContentSize().height));
	addChild(sp0,0);

	// 静态动画
	idle.reserve(1);
	idle.pushBack(frame0);

	// 攻击动画
	attack.reserve(17);

	for (int i = 0; i < 17; i++) {
		auto frame = SpriteFrame::createWithTexture(texture, CC_RECT_PIXELS_TO_POINTS(Rect(113*i,0,113,113)));
		attack.pushBack(frame);
	}

	// 可以仿照攻击动画
	// 死亡动画(帧数：22帧，高：90，宽：79）
	auto texture2 = Director::getInstance()->getTextureCache()->addImage("$lucia_dead.png");
	for (int i = 0; i < 22; i++) {
		auto frame = SpriteFrame::createWithTexture(texture2, CC_RECT_PIXELS_TO_POINTS(Rect(79 * i, 0, 79, 90)));
		dead.pushBack(frame);
	}


	// 运动动画(帧数：8帧，高：101，宽：68）
	auto texture3 = Director::getInstance()->getTextureCache()->addImage("$lucia_forward.png");
	run.reserve(8);
	for (int i = 0; i < 4; i++) {
		auto frame = SpriteFrame::createWithTexture(texture3, CC_RECT_PIXELS_TO_POINTS(Rect(68 * i, 0, 65, 101)));
		run.pushBack(frame);
	}
	return true;
}


void HelloWorld::update(float dt)
{
	hitByMonster();
	if(dtime>0)
	dtime -= dt;
	char* mtime = new char[10];
	//此处只是显示分钟数和秒数  自己可以定义输出时间格式  
	sprintf(mtime, "%d ", (int)dtime);
	time->setString(mtime);
}

void HelloWorld::updateCustom(float dt)
{
	auto fac = Factory::getInstance();
	auto m = fac->createMonster();
	float x = random(origin.x, visibleSize.width);
	float y = random(origin.y, visibleSize.height);
	m->setPosition(x, y);
	addChild(m, 3);
	Vec2 playerPos = player->getPosition();
	fac->moveMonster(playerPos,2.0f);
}

void HelloWorld::hitByMonster() {
	auto fac = Factory::getInstance();
	Sprite* collision = fac->collider(player->getBoundingBox());
	if (collision != NULL) {
		fac->removeMonster(collision);
		if (perc >= 20) {
			perc -= 20;
			auto p = CCProgressTo::create(1.0f, perc);
			pT->runAction(p);
		}
		auto deadanimation = Animation::createWithSpriteFrames(dead, 0.1f);
		auto deadanimate = Animate::create(deadanimation);
		auto idleanimation = Animation::createWithSpriteFrames(idle, 0.1f);
		auto idleanimate = Animate::create(idleanimation);
		player->runAction(Sequence::create(deadanimate, idleanimate, NULL));
	}		
}

bool HelloWorld::attackMonster() {
	Rect playerRect = player->getBoundingBox();
	Rect attackRect = Rect(playerRect.getMinX() - 40, playerRect.getMinY(), playerRect.getMaxX() - playerRect.getMinX() + 80,
							playerRect.getMaxY() - playerRect.getMinY());
	auto fac = Factory::getInstance();
	Sprite* collision = fac->collider(attackRect);
	if (collision != NULL) {
		fac->removeMonster(collision);
		return true;
		}
	return false;
}


void HelloWorld::yClickCallback(cocos2d::Ref * pSender)
{
	if (player->getNumberOfRunningActions() >= 1) {
		return;
	}	
	if (attackMonster()) {
		if (perc < 100) perc += 20;
		auto p = CCProgressTo::create(1.0f, perc);
		pT->runAction(p);
	}
	auto attackanimation = Animation::createWithSpriteFrames(attack, 0.1f);
	auto attackanimate = Animate::create(attackanimation);
	auto idleanimation = Animation::createWithSpriteFrames(idle, 0.1f);
	auto idleanimate = Animate::create(idleanimation);
	player->runAction(Sequence::create(attackanimate, idleanimate, NULL));
	Rect playerRect = player->getBoundingBox();
	Rect attackRect = Rect(playerRect.getMinX() - 40, playerRect.getMinY(), playerRect.getMaxX() - playerRect.getMinX() + 80,
		playerRect.getMaxY() - playerRect.getMinY());
}

void HelloWorld::wClickCallback(cocos2d::Ref * pSender)
{
	auto runanimation = Animation::createWithSpriteFrames(run,0.1f);  
	auto x = player->getPosition().x;
	auto y = player->getPosition().y + 40;
	if (y > (Director::getInstance()->getVisibleSize().height)) {
		y = Director::getInstance()->getVisibleSize().height;
	}
	auto runanimate = Animate::create(runanimation);
	auto front = MoveTo::create(0.4f, Vec2(x,y));
	auto runaction = Repeat::create(runanimate, 0.4f);
	player->runAction(front);player->runAction(runanimate);		
}

void HelloWorld::aClickCallback(cocos2d::Ref * pSender)
{
	auto runanimation = Animation::createWithSpriteFrames(run, 0.1f);
	auto x = player->getPosition().x - 40;
	auto y = player->getPosition().y;
	if (x<0) {
		x = 0;
	}
	auto runanimate = Animate::create(runanimation);
	auto front = MoveTo::create(0.4f, Vec2(x, y));
	auto runaction = Repeat::create(runanimate, 0.4f);
	player->setFlippedX(true);
	player->runAction(front); 
	player->runAction(runanimate);
}

void HelloWorld::sClickCallback(cocos2d::Ref * pSender)
{
	auto runanimation = Animation::createWithSpriteFrames(run, 0.1f);
	auto x = player->getPosition().x;
	auto y = player->getPosition().y - 40;
	if (y < 0) {
		y = 0;
	}
	auto runanimate = Animate::create(runanimation);
	auto front = MoveTo::create(0.4f, Vec2(x, y));
	auto runaction = Repeat::create(runanimate, 0.4f);
	player->runAction(front); player->runAction(runanimate);
}

void HelloWorld::dClickCallback(cocos2d::Ref * pSender)
{
	auto runanimation = Animation::createWithSpriteFrames(run, 0.1f);
	auto x = player->getPosition().x + 40;
	auto y = player->getPosition().y;
	if (x > (Director::getInstance()->getVisibleSize().width)) {
		x = Director::getInstance()->getVisibleSize().width;
	}
	auto runanimate = Animate::create(runanimation);
	auto front = MoveTo::create(0.4f, Vec2(x, y));
	auto runaction = Repeat::create(runanimate, 0.4f);
	player->setFlippedX(false);
	player->runAction(front);
	player->runAction(runanimate);
}
