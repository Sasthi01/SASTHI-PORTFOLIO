#include "Home.h"
//Forward declaration
#include "Game.h"


//Constructor
Home::Home(sf::RenderWindow& window, Game& game) : window(window) , game(game)//reference to window
{
	/* sets all graphics */ 
	loadFonts();
	setBackground();
	setTitle();
	setStartText();
	setExitText();
	loadSounds();

	//plays the home screen background music
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "LobbyMusic");
}

//Destructor
Home::~Home()
{

}

//private functions
//loads all the sounds in the respective category
void Home::loadSounds()
{
	Audio::getInstance().loadSound(SoundEffects::UIButtons);
	Audio::getInstance().loadSound(BackgroundAudio::BackgroundSounds);
}

//when start button is clicked, this code will run to erase LobbyMusic from memory, play the UIButton sound and set the ambience for the basement
void Home::HandleChangeEventAudio()
{
	Audio::getInstance().playSound(SoundEffects::UIButtons, "UIButton1");
	sf::sleep(sf::milliseconds(200));                               //delaying the program so it doesnt erase the sound faster than it can play
	Audio::getInstance().eraseSound("UIButton1");
	Audio::getInstance().eraseSound("LobbyMusic");
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "StartingAmbience");
	Audio::getInstance().setSoundVolume("StartingAmbience", 20.0f);
}
void Home::loadFonts()
{
	textFont.loadFromFile("Assets/Fonts/pixel-game/Pixel Game.otf");
}

void Home::setBackground()
{
	background.loadFromFile("Assets/BackgroundImages/Home.jpg");
	backgroundImage.setTexture(background);
}

void Home::setTitle()
{
	title.setFont(textFont);
	title.setString("TRAPPED");
	title.setCharacterSize(300);
	title.setFillColor(sf::Color::White);
	title.setPosition({ 200.f, 400.f });
}

void Home::setStartText()
{
	startText.setFont(textFont);
	startText.setString("START");
	startText.setCharacterSize(50);
	startText.setFillColor(sf::Color::White);
	startText.setPosition({ 600.f, 400.f });
}

void Home::setExitText()
{
	exitText.setFont(textFont);
	exitText.setString("EXIT");
	exitText.setCharacterSize(50);
	exitText.setFillColor(sf::Color::White);
	exitText.setPosition({ 620.f, 450.f });
}


void Home::handleInput(sf::Vector2i& mousePos)
{
	/* handles input when button is pressed on Home screen
	takes us to the next room/state */
	if (startText.getGlobalBounds().contains(sf::Vector2f(mousePos)))
	{
		HandleChangeEventAudio();
		game.setState(Game::State::Intro);
	}
	else if (exitText.getGlobalBounds().contains(sf::Vector2f(mousePos)))
	{
		Audio::getInstance().playSound(SoundEffects::UIButtons, "UIButton1");
		window.close();
	}
}

//can be used for updating game logic
void Home::update()
{
	sf::Vector2i mousePos = sf::Mouse::getPosition(window);

	// Check if the left mouse button is pressed
	if (sf::Mouse::isButtonPressed(sf::Mouse::Left)) {
		handleInput(mousePos);
	}
}

//used to display graphics
void Home::render()
{
	//not putting clear as the game class clears the window
	//draw graphic objects on window
	window.draw(backgroundImage);
	window.draw(title);
	window.draw(startText);
	window.draw(exitText);
	//game class will display
}
