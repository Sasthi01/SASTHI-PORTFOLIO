#pragma once
#include <string>
#include <vector>
#include "Option.h"

using namespace std;
class Option;

class Question
{

private:
	vector<Option> options;//Stores the options for this question
	int numOptions;//Number of possible options, this varies
	string prompt;// The text that will be displayed for the question
	string requiredItem;

public:
	void displayOptions();
	int chooseOption(int option);
	void displayPrompt();
	Question(string prompt);
	void setNumOptions(int numOptions);
	void addOption(Option o);
	Question();
	string getPrompt();
	void setRequiredItem(string itemName);
	string getRequiredItem();
	bool requiresItem();
	string toString();
	vector<Option> getOptions() const;







};

