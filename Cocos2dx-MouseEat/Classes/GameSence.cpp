#include "GameSence.h"

USING_NS_CC;

Scene* GameSence::createScene()
{
	// 'scene' is an autorelease object
	auto scene = Scene::create();

	// 'layer' is an autorelease object
	auto layer = GameSence::create();

	// add layer as a child to scene
	scene->addChild(layer);

	// return the scene
	return scene;
}

// on "init" you need to initialize your instance
bool GameSence::init()
{	
	Size visibleSize = Director::getInstance()->getVisibleSize();
	Vec2 origin = Director::getInstance()->getVisibleOrigin();
	if (!Layer::init())
	{
		return false;
	}
	else {
		stoneLayer = Layer::create();
		mouseLayer = Layer::create();
		//设置锚点
		stoneLayer->setAnchorPoint(ccp(0, 0));
		mouseLayer->setAnchorPoint(ccp(0, 0));
		stoneLayer->setPosition(ccp(0, 0));
		mouseLayer->setPosition(ccp(0, visibleSize.height / 2));
		//添加精灵在stonelayer
		stone = Sprite::create("stone.png");
		stone->setPosition(ccp(560, 480));
		stoneLayer->addChild(stone,1);

		auto bg = Sprite::create("level-background-0.jpg");
		bg->setPosition(ccp(visibleSize.width/2, visibleSize.height/2));
		stoneLayer->addChild(bg, 0);

		//添加精灵在mouselayer
		mouse = Sprite::createWithSpriteFrameName("gem-mouse-0.png");
		Animate* mouseAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("mouseAnimation"));
		mouse->runAction(RepeatForever::create(mouseAnimate));
		mouse->setPosition(ccp(visibleSize.width / 2, 0));
		mouseLayer->addChild(mouse, 1);
		//添加层
		addChild(stoneLayer);
		addChild(mouseLayer);
	}
	//add label
	auto shootLabel = Label::createWithTTF("Shoot", "fonts/Marker Felt.ttf", 70);
	//创建另一个menuitem
	auto shootItem = MenuItemLabel::create(shootLabel, CC_CALLBACK_1(GameSence::shootMenuCallback, this));
	shootItem->setPosition(ccp(800, 480));
	//create menu, it's an autorelease object
	auto shootmenu = Menu::create(shootItem, NULL);
	shootmenu->setPosition(Vec2::ZERO);
	this->addChild(shootmenu, 1);
	mPlayBounds = CCRectMake(800, 480, 70, 70);

	//add touch listener
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->setSwallowTouches(true);
	listener->onTouchBegan = CC_CALLBACK_2(GameSence::onTouchBegan, this);
	Director::getInstance()->getEventDispatcher()->addEventListenerWithSceneGraphPriority(listener, this);

	return true;
}

bool GameSence::onTouchBegan(Touch *touch, Event *unused_event) {
	//auto touchlocation = touch->getLocation();
	Size visibleSize = Director::getInstance()->getVisibleSize();
	auto touchlocation = touch->getLocation();
	auto cheese = Sprite::create("cheese.png");
	cheese->setPosition(touchlocation);
	auto nl = cheese->convertToNodeSpaceAR(mouseLayer->getPosition());
	bool flag = mPlayBounds.containsPoint(touchlocation);
	if (flag == false) {	
		auto mmoveTo = MoveTo::create(1.5f,Vec2(touchlocation.x,touchlocation.y - visibleSize.height/2));
		auto fadeOut = FadeOut::create(6.0f);
		mouse->runAction(mmoveTo);
		cheese->runAction(fadeOut);
		this->addChild(cheese,0);
	}
	return true;
}
void GameSence::shootMenuCallback(Ref* pSender) {
	Size visibleSize = Director::getInstance()->getVisibleSize();
	auto wl = mouse->convertToWorldSpaceAR(this->getPosition());

	//石头移动
	auto stoneclone = Sprite::create("stone.png");
	stoneclone->setPosition(ccp(560, 480));
	stoneLayer->addChild(stoneclone, 1);
	auto moveTo = MoveTo::create(1.0f,wl);
	auto fadeOut = FadeOut::create(1.0f);
	auto seq = Sequence::create(moveTo, fadeOut,nullptr);
	stoneclone->runAction(seq);

	//鼠移动
	auto posx = RandomHelper::random_int(0, (int)visibleSize.width);
	auto posy = RandomHelper::random_int(-(int)visibleSize.height / 2,(int)visibleSize.height/2);
	auto mmoveTo = MoveTo::create(0.8f, Vec2(posx,posy));
	mouse->runAction(mmoveTo);

	//留下钻石
	auto diamond = Sprite::create("diamond.png");
	diamond->setPosition(wl);
	stoneLayer->addChild(diamond, 1);
}
