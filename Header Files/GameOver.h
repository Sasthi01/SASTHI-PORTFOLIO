#pragma once
#include <SFML/Graphics.hpp>
#include <iostream>
#include "Audio.h"
class Game;

/*Class to display game over */

class GameOver
{
public:
	//constructor/ destructor
	GameOver(sf::RenderWindow& window, Game& game);
	~GameOver();

	//Functions
	void handleInput(sf::Vector2i& mousePos);
	void update();
	void render();

private:
	//variables
	//reference to window
	sf::RenderWindow& window;
	//reference to game
	Game& game;
	//graphic variables
	sf::Texture background;
	sf::Sprite backgroundImage;
	sf::Font textFont;
	sf::Text title;
	sf::Text finalResult;
	sf::Text startText;
	sf::Text exitText;

	// Progress variables and methods to provide final feedback to the player 
	const int TOTALGAMEQUESTIONS = 43; // The total number of questions that increase progress throughout the game
	double percentageCompleted; // The percentage of the game that was completed
	int calculatePercentageCompleted(); // Calculates and returns the final percentage of the game completed


	//private functions
	void loadFonts();
	void setFinalResult();
	void setBackground();
	void setTitle();
	void setExitText();
	void setStartText();
	void HandleChangeEventAudio();
	void loadSounds();
};
