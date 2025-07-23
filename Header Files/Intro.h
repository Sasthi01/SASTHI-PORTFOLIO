#pragma once
#include <SFML/Graphics.hpp>
#include <iostream>
#include "Audio.h"
class Game;

/*Class to display backstory and tutorial */

class Intro
{
public:
	//constructor/ destructor
	Intro(sf::RenderWindow& window, Game& game);
	~Intro();

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
	sf::Text nextText;
	int count = 1;

	//private functions
	void loadFonts();
	void setBackground();
	void setNextText();
	void loadSounds();
	void HandleChangeEventAudio();
};

