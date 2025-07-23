#pragma once

#include <SFML/Graphics.hpp>
#include <vector>
#include "option.h"
#include <iostream>
#include "TextToSpeech.h"

using namespace std;

class DialogueBox {
private:
	sf::RectangleShape textBackground;
	sf::Text dialogueText;
	//work
	vector<sf::Text> optionTexts;
	vector<sf::RectangleShape> optionBoxes;
	//layout constants being used
	const sf::Vector2f dialogueBoxSize{ 1180.f, 160.f };
	const sf::Vector2f dialogueBoxPos{ 60.f, 480.f };
	//buttons
	const sf::Vector2f buttonSize{ 220.f, 60.f };
	const float buttonY = dialogueBoxPos.y + dialogueBoxSize.y + 5.f;
	const float buttonSpacing = 20.f;
	//font sizes
	const unsigned int dialogueInitialFontSize = 24;
	const unsigned int dialogueMinFontSize = 14;
	const unsigned int buttonInitialFontSize = 20;
	const unsigned int buttonMinFontSize = 14;
	//padding (space between text and box edges)
	const float padding = 10.f;

	sf::Font font;
	TextToSpeech textToSpeech;
	sf::Texture SpeakerTexture;       // Texture for the broken heart icon
	sf::Sprite SpeakerSprite;         // Sprite to render the broken heart icon

	static atomic<bool> isSpeaking; //atomic ensures its thread safe
public:

	
	//constructor initialized with ref to a sfml font
	DialogueBox();

	//set and display main text
	void setText(const string& text);

	//set the dialogue choices using a vector of Option objects
	//if no options avaialable, a continue option is added by degault
	void setChoices(const std::vector<Option>& options);

	int handleInput(const sf::Vector2i& mousePos); // updated handleEvent to this
	void render(sf::RenderWindow& window); // updated draw to this

	//text wrapping helper class 
	sf::Text textWrap(const std::string& text, sf::Vector2f boxSize, unsigned int initialSize, unsigned int minSize);
	void setTextToSpeech(TextToSpeech &textToSpeech);


};
