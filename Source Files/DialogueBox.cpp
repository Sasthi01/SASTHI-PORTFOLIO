#include "DialogueBox.h"
using namespace std;
atomic<bool> DialogueBox::isSpeaking = false;

DialogueBox::DialogueBox():textToSpeech() {
	font.loadFromFile("Assets/Fonts/roboto/Roboto-Regular.ttf");
	
	//background for dialogue text
	textBackground.setSize(dialogueBoxSize);
	textBackground.setFillColor(sf::Color(50, 50, 50, 200));
	textBackground.setOutlineColor(sf::Color::White);
	textBackground.setOutlineThickness(2.f);
	textBackground.setPosition(dialogueBoxPos);

	//font for text
	dialogueText.setFont(font);
	dialogueText.setFillColor(sf::Color::White);
		
}

void DialogueBox::setText(const string& text) {
	//ajust text so that it fits inside the box
	dialogueText = textWrap(
		text,
		dialogueBoxSize - sf::Vector2f(padding * 2, padding * 2),
		dialogueInitialFontSize,
		dialogueMinFontSize);

	//positioning the text inside to be centered (vertically)
	sf::FloatRect bounds = dialogueText.getLocalBounds();
	dialogueText.setPosition(
		dialogueBoxPos.x + padding,
		dialogueBoxPos.y + (dialogueBoxSize.y - bounds.height) / 2.f - bounds.top
	);
	

}

//setting up the options under the dialogue box
void DialogueBox::setChoices(const vector<Option>& options) {
	//clear any old choices if there are
	optionTexts.clear();
	optionBoxes.clear();

	vector<string> choiceStrings;

	if (options.empty()) { //continue
		choiceStrings.emplace_back("Continue"); //adds this at the end of the vector
	}
	else // convert each option to a string
	{
		for (const auto& opt : options)
			choiceStrings.emplace_back(opt.toString());
	}

	size_t numChoices = choiceStrings.size();
	float totalWidth = numChoices * buttonSize.x + (numChoices - 1) * buttonSpacing;
	float startX = (1280.f - totalWidth) / 2.f;

	for (size_t i = 0; i < numChoices; ++i) {
		sf::RectangleShape box(buttonSize);
		box.setFillColor(sf::Color::Black);
		box.setOutlineColor(sf::Color::White);
		box.setOutlineThickness(2.f);
		box.setPosition(startX + i * (buttonSize.x + buttonSpacing), buttonY);

		//text inside box
		sf::Text text = textWrap(
			choiceStrings[i],
			buttonSize - sf::Vector2f(padding * 2, padding * 2),
			buttonInitialFontSize,
			buttonMinFontSize);

		//center the text inside box
		sf::FloatRect bounds = text.getLocalBounds();
		text.setPosition(
			box.getPosition().x + (buttonSize.x - bounds.width) / 2.f - bounds.left,
			box.getPosition().y + (buttonSize.y - bounds.height) / 2.f - bounds.top
		);

		//add to vectors
		optionBoxes.push_back(box);
		optionTexts.push_back(text);
	}

}

/*
SFML doesnt support auto text wrapping so we need to have a wrapText helper class to make sure the
text doesnt go outside the textbox
*/

//Wraps text for both dialogue box and choice buttons
sf::Text DialogueBox::textWrap(const string& text, sf::Vector2f boxSize,
	unsigned int initialSize, unsigned int minSize)
{
	sf::Text tempText("", font);
	unsigned int fontSize = initialSize;

	//keep reducing the font size until the wrapped text fits within the box vertically
	while (fontSize >= minSize) {
		tempText.setCharacterSize(fontSize); //set font size for measurement

		//strings will be used for building the text
		string wrappedText; //final result
		string word; //current word
		string line; //current line

		tempText.setString(""); //clears any previous content

		//go through each character in the input text
		for (size_t i = 0; i < text.length(); ++i) {
			char c = text[i];

			//if a space or newline is hit then its the end of a word
			if (c == ' ' || c == '\n') {
				string testLine;
				if (line.empty()) {
					testLine = word;
				}
				else {
					testLine = line + " " + word;
				}

				//measure width of line
				tempText.setString(testLine);
				float lineWidth = tempText.getLocalBounds().width;

				//if it exceeds the width of the box, wrap to a new line
				if (lineWidth > boxSize.x) {
					wrappedText += line + '\n'; //save current line and go to next
					line = word; //start new line with current word
				}
				else {
					line = testLine;//add word to line
				}

				//handles line breaks in the original text
				if (c == '\n') {
					wrappedText += line + '\n';
					line.clear();
				}

				word.clear();
			}
			else {
				//builds up the current word 
				word += c;
			}
		}

		//handles any leftover word/line
		if (!word.empty()) {
			string testLine;
			if (line.empty()) {
				testLine = word;
			}
			else {
				testLine = line + " " + word;
			}


			tempText.setString(testLine);
			if (tempText.getLocalBounds().width > boxSize.x) {
				wrappedText += line + '\n' + word;
			}
			else {
				wrappedText += testLine;
			}
		}
		else {
			wrappedText += line;
		}

		//checks height
		tempText.setString(wrappedText);
		if (tempText.getLocalBounds().height <= boxSize.y) {
			break;
		}

		//text didn't fit vertically, so it gets decremented
		fontSize--;
	}
	return tempText;
}




int DialogueBox::handleInput(const sf::Vector2i& mousePos) {
	//prevents overlapping of speech
	if (!isSpeaking) {
		if (SpeakerSprite.getGlobalBounds().contains(sf::Vector2f(mousePos))) { //only plays speech if icon is clicked
			isSpeaking = true;

			//safer to set the text outside of thread
			string text = dialogueText.getString();
			//thread used so that textToSpeech does not pause game screen.
			//thread runs in background to prevent interference with game flow
			//User can wait for it to finish or continue with game while it runs
			thread([this, text]() {
				textToSpeech.speak(text); 
				isSpeaking = false;
			}).detach();
		}
	}
	for (size_t i = 0; i < optionBoxes.size(); ++i) {
		//if mouse position is inside an option

		if (optionBoxes[i].getGlobalBounds().contains(sf::Vector2f(mousePos))) {


			//return the index
			return static_cast<int>(i);  //converts size_t index to int
		}
	}
	return -1;
}

//renders dialogue box
void DialogueBox::render(sf::RenderWindow& window) {

	SpeakerTexture.loadFromFile("Assets/Icons/Speaker.png");
	SpeakerSprite.setTexture(SpeakerTexture);
	SpeakerSprite.setScale(0.5f, 0.5f); 
	SpeakerSprite.setPosition({ 1200.f, 435.f });

	window.draw(textBackground);
	window.draw(dialogueText);
	window.draw(SpeakerSprite);

	for (size_t i = 0; i < optionBoxes.size(); ++i) {
		window.draw(optionBoxes[i]);
		window.draw(optionTexts[i]);
	}

}
