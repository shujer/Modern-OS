#include "MenuSence.h"
#include "GameSence.h"
USING_NS_CC;

Scene* MenuSence::createScene()
{
    // 'scene' is an autorelease object
    auto scene = Scene::create();

    // 'layer' is an autorelease object
    auto layer = MenuSence::create();

    // add layer as a child to scene
    scene->addChild(layer);

    // return the scene
    return scene;
}

// on "init" you need to initialize your instance
bool MenuSence::init()
{

    if ( !Layer::init() )
    {
        return false;
    }

	Size visibleSize = Director::getInstance()->getVisibleSize();
	Vec2 origin = Director::getInstance()->getVisibleOrigin();

	//创建一个menuitem
	auto closeItem = MenuItemImage::create("start-0.png","start-1.png",CC_CALLBACK_1(MenuSence::menuCloseCallback, this));

	closeItem->setPosition(Vec2(origin.x + visibleSize.width - 200, visibleSize.height / 2 + origin.y - 130));

	// create menu, it's an autorelease object
	auto menu = Menu::create(closeItem, NULL);
	menu->setPosition(Vec2::ZERO);
	this->addChild(menu, 2);

	//stone
	auto stone = Sprite::create("menu-start-gold.png");
	stone->setPosition(Vec2(origin.x + visibleSize.width - 200, visibleSize.height / 2 + origin.y - 180));
	this->addChild(stone, 1);

	auto bg_sky = Sprite::create("menu-background-sky.jpg");
	bg_sky->setPosition(Vec2(visibleSize.width / 2 + origin.x, visibleSize.height / 2 + origin.y + 150));
	this->addChild(bg_sky, 0);

	auto bg = Sprite::create("menu-background.png");
	bg->setPosition(Vec2(visibleSize.width / 2 + origin.x, visibleSize.height / 2 + origin.y - 60));
	this->addChild(bg, 0);

	auto title = Sprite::create("gold-miner-text.png");
	title->setPosition(origin + Vec2(visibleSize.width / 2, visibleSize.height / 2 + 180));
	this->addChild(title, 0);

	auto miner = Sprite::create("menu-miner.png");
	miner->setPosition(Vec2(150 + origin.x, visibleSize.height / 2 + origin.y - 60));
	this->addChild(miner, 1);

	auto leg = Sprite::createWithSpriteFrameName("miner-leg-0.png");
	Animate* legAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("legAnimation"));
	leg->runAction(RepeatForever::create(legAnimate));
	leg->setPosition(110 + origin.x, origin.y + 102);
	this->addChild(leg, 1);

    return true;
}

void  MenuSence::menuCloseCallback(Ref* pSender)
{
	TransitionRotoZoom *tranScene = TransitionRotoZoom::create(2.0,GameSence::createScene() );
	Director::getInstance()->replaceScene(tranScene);
}


