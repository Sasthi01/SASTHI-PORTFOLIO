#include "Intro.h"
#include "Game.h"

Intro::Intro(sf::RenderWindow& window, Game& game) : window(window), game(game)
{
	/* sets all graphics */
	loadFonts();
	setBackground();
	setNextText();
	loadSounds();

	//plays the intro screen background music
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "LobbyMusic");
}

Intro::~Intro()
{
}

void Intro::handleInput(sf::Vector2i& mousePos)
{
	/* handles input when button is pressed on intro screen
	takes us to the next room/state */
	if (nextText.getGlobalBounds().contains(sf::Vector2f(mousePos)))
	{
		HandleChangeEventAudio();
		//only move to new room once you complete introduction (2 screens for intro)
		if (count >= 2) {
			game.setState(Game::State::Basement);
		}
		else {
			count++;
			setBackground();
		}
	}
}

void Intro::update()
{
	sf::Vector2i mousePos = sf::Mouse::getPosition(window);

	// Check if the left mouse button is pressed
	if (sf::Mouse::isButtonPressed(sf::Mouse::Left)) {
		handleInput(mousePos);
	}
}

void Intro::render()
{
	window.draw(backgroundImage);
	window.draw(nextText);
}

void Intro::loadFonts()
{
	textFont.loadFromFile("Assets/Fonts/pixel-game/Pixel Game.otf");
}

void Intro::setBackground()
{
	background.loadFromFile("Assets/BackgroundImages/Intro"+ to_string(count) +".jpg");
	backgroundImage.setTexture(background);
}

void Intro::setNextText()
{
	nextText.setFont(textFont);
	nextText.setString("CONTINUE");
	nextText.setCharacterSize(50);
	nextText.setFillColor(sf::Color::White);
	nextText.setPosition({ 1100.f, 650.f });
}

void Intro::loadSounds()
{
	Audio::getInstance().loadSound(SoundEffects::UIButtons);
	Audio::getInstance().loadSound(BackgroundAudio::BackgroundSounds);
}

void Intro::HandleChangeEventAudio()
{
	Audio::getInstance().playSound(SoundEffects::UIButtons, "UIButton1");
	sf::sleep(sf::milliseconds(200));                               //delaying the program so it doesnt erase the sound faster than it can play
	Audio::getInstance().eraseSound("UIButton1");
	//changes audio for next room 
	if (count >= 2) {
		Audio::getInstance().eraseSound("LobbyMusic");
		Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "StartingAmbience");
		Audio::getInstance().setSoundVolume("StartingAmbience", 20.0f);
	}
}
