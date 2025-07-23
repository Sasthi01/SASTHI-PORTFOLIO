#include "Game.h"
#include "TextToSpeech.h"

//private functions
void Game::initializeVariables()
{
	currentState = State::Home;
	setWin(false);
	inventory = new Inventory<Item>();
	home = new Home(window, *this);
	intro = nullptr;
	basement = nullptr;
	stairs = nullptr;
	livingRoom = nullptr;
	bathroom = nullptr;
	kitchen = nullptr;
	forest = nullptr;
	car = nullptr;
	gameOver = nullptr;
}

void Game::initializeWindow()
{
	window.create(sf::VideoMode({ 1280, 720 }), "Trapped", sf::Style::Fullscreen);

}

//Constructor
Game::Game()
{
	initializeVariables();
	initializeWindow();





}

//Destructor
Game::~Game()
{
	//deletes objects if it exists
	if (home)
	{
		delete home;
	}

	if (intro)
	{
		delete intro;
	}

	if (basement)
	{
		delete basement;
	}

	if (stairs)
	{
		delete stairs;
	}

	if (bathroom)
	{
		delete bathroom;
	}

	if (livingRoom)
	{
		delete livingRoom;
	}

	if (kitchen)
	{
		delete kitchen;
	}

	if (forest)
	{
		delete forest;
	}

	if (car)
	{
		delete car;
	}

	if (gameOver)
	{
		delete gameOver;
	}

	if (inventory) 
	{
		delete inventory;
	}
}

//Functions
/*Updates all game logic */
void Game::update()
{
	updateEvents();

	//updates logic based on which state we are in
	switch (currentState) {
	case State::Home:
		home->update();
		break;
	case State::Intro:
		intro->update();
		break;
	case State::Basement:
		basement->update();
		break;
	case State::Stairs:
		stairs->update();
		break;
	case State::Kitchen:
		kitchen->update();
		break;
	case State::LivingRoom:
		livingRoom->update();
		break;
	case State::Bathroom:
		bathroom->update();
		break;
	case State::Forest:
		forest->update();
		break;
	case State::Car:
		car->update();
		break;
	case State::GameOver:
		gameOver->update();
		break;
	default:
		break;
	}
}

/* Render game objects. Used to visualize game */
void Game::render()
{
	//clear window
	window.clear();

	//draw game based on which room we in
	switch (currentState) {
	case State::Home:
		home->render(); 
		break;
	case State::Intro:
		intro->render();
		break;
	case State::Basement:
		basement->render(); 
		break;
	case State::Stairs:
		stairs->render();
		break;
	case State::Kitchen:
		kitchen->render();
		break;
	case State::LivingRoom:
		livingRoom->render();
		break;
	case State::Bathroom:
		bathroom->render();
		break;
	case State::Forest:
		forest->render();
		break;
	case State::Car:
		car->render();
		break;
	case State::GameOver:
		gameOver->render();
		break;
	default:
		break;
	}

	//display window
	window.display();
}

/* Handles events */
void Game::updateEvents()
{
	//Event polling
	while (window.pollEvent(event))
	{
		switch (event.type)
		{
		case sf::Event::Closed:
			window.close();
			break;
		case sf::Event::KeyPressed:
			if (event.key.code == sf::Keyboard::Escape)
			{
				window.close();
			}
			break;
		default:
			break;
		}
	}
}

//Accesors
const bool Game::getRunning() const
{
	return window.isOpen();
}

void Game::setState(State state)
{
	currentState = state;
	switch (currentState) {
	case State::Home:
		if (!home)
		{
			home = new Home(window, *this);
		}
		break;
	case State::Intro:
		if (!intro)
		{
			intro = new Intro(window, *this);
		}
		break;
	case State::Basement:
		if (!basement)
		{
			basement = new Basement(window, *this, *inventory);
			if (home) {
				delete home;
				home = nullptr;
			}
		}
		if (gameOver) {
			//ensures that if you restart game it will delete all the existing rooms except for basement
			if (stairs)
			{
				delete stairs;
				stairs = nullptr;
			}
			if (bathroom)
			{
				delete bathroom;
				bathroom = nullptr;
			}
			if (livingRoom)
			{
				delete livingRoom;
				livingRoom = nullptr;
			}
			if (kitchen)
			{
				delete kitchen;
				kitchen = nullptr;
			}
			if (forest)
			{
				delete forest;
				forest = nullptr;
			}
			if (car)
			{
				delete car;
				car = nullptr;
			}
			inventory->clear();
		}
		basement->intializeQuestionVariables();
		break;
	case State::Stairs:
		if (!stairs)
		{
			stairs = new Stairs(window, *this, *inventory);
			if (gameOver) {
				delete gameOver;
				gameOver = nullptr;
			}
			if (intro) {
				delete intro;
				intro = nullptr;
			}
		}
		else {
			stairs->intializeQuestionVariables();
		}
		break;
	case State::Kitchen:
		if (!kitchen) {
			kitchen = new Kitchen(window, *this, *inventory);
			if (basement)
			{
				delete basement;
				basement = nullptr;
			}
			if (bathroom)
			{
				delete bathroom;
				bathroom = nullptr;
			}
		}
		break;
	case State::LivingRoom:
		if (!livingRoom) {
			livingRoom = new LivingRoom(window, *this, *inventory);
			if (basement)
			{
				delete basement;
				basement = nullptr;
			}
			if (bathroom)
			{
				delete bathroom;
				bathroom = nullptr;
			}
		}
		break;
	case State::Bathroom:
		if (!bathroom) {
			bathroom = new Bathroom(window, *this, *inventory);
			if (basement)
			{
				delete basement;
				basement = nullptr;
			}
		}
		bathroom->intializeQuestionVariables();
		break;
	case State::Forest:
		if (!forest) {
			forest = new Forest(window, *this, *inventory);
			delete stairs;
			stairs = nullptr;
		}
		break;
	case State::Car:
		if (!car) {
			car = new Car(window, *this, *inventory);
			delete stairs;
			stairs = nullptr;
		}
		break;
	case State::GameOver:
		if (!gameOver) {
			gameOver = new GameOver(window, *this);
		}
		break;
	default:
		break;
	}
}

void Game::setWin(bool w)
{
	this->win = w;
}

bool Game::isWin() const
{
	return this->win;
}



