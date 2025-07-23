#pragma once
#include <SFML/Graphics.hpp>
#include <iostream>
#include "Audio.h"
class Game;

/*Class to display main menu */

class Home
{
public:
	//constructor/ destructor
	Home(sf::RenderWindow& window, Game& game);
	~Home();
	
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
	sf::Text startText;
	sf::Text exitText;

	//private functions
	void loadFonts();
	void setBackground();
	void setTitle();
	void setStartText();
	void setExitText();
	void loadSounds();
	void HandleChangeEventAudio();
};

