#pragma once //same purpose as ifndef

#include <SFML/Graphics.hpp>
#include <SFML/System.hpp>
#include <SFML/Window.hpp>
#include <SFML/Audio.hpp>
#include <SFML/Network.hpp>
#include "Home.h"
#include "Intro.h"
#include "Basement.h"
#include "Stairs.h"
#include "LivingRoom.h"
#include "Bathroom.h"
#include "Kitchen.h"
#include "Forest.h"
#include "Car.h"
#include "Inventory.h"
#include "GameOver.h"
#include "TextToSpeech.h"

/* Game engine class */

class Game
{
public:
	//constructor/destructor
	Game();
	~Game();

	//Functions
	void update();
	void render();
	void updateEvents();

	//Accessors
	const bool getRunning() const;

	//game state functions - tells us which room we working with
	enum class State
	{
		Home,
		Intro,
		Basement,
		Stairs,
		Bathroom,
		Kitchen,
		LivingRoom,
		Forest,
		Car,
		GameOver
	};
	void setState(State state);
	void setWin(bool w);
	bool isWin() const;

	

private:
	//variables
	sf::RenderWindow window;
	sf::Event event;
	bool win;

	//create room variables
	Home* home;
	Intro* intro;
	Basement* basement;
	Stairs* stairs;
	LivingRoom* livingRoom;
	Bathroom* bathroom;
	Kitchen* kitchen;
	Forest* forest;
	Car* car;
	GameOver* gameOver;

	// Inventory
	Inventory<Item>* inventory; // pointer to inventory

	//game state
	State currentState;



	//private functions
	void initializeVariables();
	void initializeWindow();
};

