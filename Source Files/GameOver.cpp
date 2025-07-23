#include "GameOver.h"
//Forward declaration
#include "Game.h"

GameOver::GameOver(sf::RenderWindow& window, Game& game) : window(window), game(game)
{
	/* sets all graphics */
	loadFonts();
	setBackground();
	setTitle();
	setStartText();
	setExitText();
	setFinalResult();
	loadSounds();

	//plays the game over screen background music
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "LobbyMusic");
}

GameOver::~GameOver()
{
}

int GameOver::calculatePercentageCompleted() {
	percentageCompleted = (static_cast<double>(ProgressBar::getAnsweredQuestions()) / TOTALGAMEQUESTIONS) * 100;
	return percentageCompleted;
}

void GameOver::loadSounds()
{
	//erasing the background sounds so that whenever theres a gameover, the background sound from that stage doesnt play in gameover screen
	Audio::getInstance().eraseSound("StageTwoAmbienceSound");
	Audio::getInstance().eraseSound("ForestAmbience");
	Audio::getInstance().eraseSound("CarAmbience");

	//reloading the background sound for the basement
	Audio::getInstance().loadSound(SoundEffects::UIButtons);
	Audio::getInstance().loadSound(BackgroundAudio::BackgroundSounds);
}

void GameOver::HandleChangeEventAudio()
{
	Audio::getInstance().playSound(SoundEffects::UIButtons, "UIButton1");
	sf::sleep(sf::milliseconds(200));                               //delaying the program so it doesnt erase the sound faster than it can play
	Audio::getInstance().eraseSound("UIButton1");
	Audio::getInstance().eraseSound("LobbyMusic");
	Audio::getInstance().playSound(BackgroundAudio::BackgroundSounds, "StartingAmbience");
	Audio::getInstance().setSoundVolume("StartingAmbience", 20.0f);
}
void GameOver::handleInput(sf::Vector2i& mousePos)
{
	// closes game when they press exit
	if (exitText.getGlobalBounds().contains(sf::Vector2f(mousePos)))
	{
		Audio::getInstance().playSound(SoundEffects::UIButtons, "UIButton1");
		window.close();
	}
	else if (startText.getGlobalBounds().contains(sf::Vector2f(mousePos))) {
		HandleChangeEventAudio();
		ProgressBar::resetAnsweredQuestions();
		game.setWin(false); 
		game.setState(Game::State::Basement);
	}
}

void GameOver::update()
{
	sf::Vector2i mousePos = sf::Mouse::getPosition(window);

	// Check if the left mouse button is pressed
	if (sf::Mouse::isButtonPressed(sf::Mouse::Left)) {
		handleInput(mousePos);
	}
	setBackground();
	setTitle();
}

void GameOver::render()
{
	//draw graphic objects on window
	window.draw(backgroundImage);
	window.draw(title);
	window.draw(startText);
	window.draw(exitText);
	window.draw(finalResult);
}

void GameOver::loadFonts()
{
	textFont.loadFromFile("Assets/Fonts/pixel-game/Pixel Game.otf");
}

void GameOver::setFinalResult() {
	finalResult.setFont(textFont);
	finalResult.setString("YOU COMPLETED: " + std::to_string(calculatePercentageCompleted()) + "%");
	finalResult.setCharacterSize(50);
	finalResult.setFillColor(sf::Color::White);
	finalResult.setPosition({ 505.f, 350.f });
}

void GameOver::setBackground()
{
	if (game.isWin()) {
		background.loadFromFile("Assets/BackgroundImages/Win.jpg"); //set background for if they win
	}
	else {
		background.loadFromFile("Assets/BackgroundImages/Lose.jpg"); // set background for if they lose
	}

	backgroundImage.setTexture(background);
}

void GameOver::setTitle()
{
	
	title.setFont(textFont);
	if (game.isWin()) {
		title.setString("YOU WIN");
		title.setPosition({ 250.f, 450.f });
	}
	else {
		title.setString("GAME OVER");
		title.setPosition({ 200.f, 450.f });
	}
	
	title.setCharacterSize(250);
	title.setFillColor(sf::Color::White);
}

void GameOver::setExitText()
{
	exitText.setFont(textFont);
	exitText.setString("EXIT");
	exitText.setCharacterSize(50);
	exitText.setFillColor(sf::Color::White);
	exitText.setPosition({ 620.f, 450.f });
}

void GameOver::setStartText()
{
	startText.setFont(textFont);
	startText.setString("TRY AGAIN");
	startText.setCharacterSize(50);
	startText.setFillColor(sf::Color::White);
	startText.setPosition({ 560.f, 400.f });
}
