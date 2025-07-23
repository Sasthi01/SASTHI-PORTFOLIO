#include "Question.h"

//Creates the question and sets the prompt

Question::Question(string prompt) {

	this->prompt = prompt;

}
//Sets the number of options for the current question
void Question::setNumOptions(int numOptions) {

	this->numOptions = numOptions;

}
//Adds the option to the vector
void Question::addOption(Option o) {
	options.push_back(o);
}

void Question::displayOptions() {

}

/*Choose an option by providing the index of the chosen option with 0 being
The first oneit will return the index of the next question*/

int Question::chooseOption(int option) {
	return options[option].getNextQ();
}
void Question::displayPrompt() {

}

Question::Question() {

}

string Question::getPrompt() {
	return prompt;
}

void Question::setRequiredItem(string itemName) {
	requiredItem = itemName;
}

string Question::getRequiredItem() {
	return requiredItem;
}

bool Question::requiresItem() {
	if (requiredItem == "none") {
		return false;
	}
	else {
		return true;
	}
}

string Question::toString() {

	string output = prompt + '\n';

	return output;

}


vector<Option> Question::getOptions() const
{
	return options;
}

