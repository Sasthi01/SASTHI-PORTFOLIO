#pragma once
#include <string>
//#include "Question.h"
//#include "Room.h"

class Room;
class Question;

using namespace std;
class Option
{

private:
	Question* nextQuestion; // Next question objext
	string text; // ?
	Room* nextRoom; // ?
	int nextQIndex; // Next question index
	bool progress; // ?

public:
	//=========NEEDS COMMENTS==============
	Option(string text);
	void setNextQIndex(int index);
	Option();
	void setProgress(bool progress);
	string toString() const; //made const
	int getNextQ();

	//getters to retrieve option information
	string getText() const; //its const because members shouldnt be modified
	int getNextQIndex() const;

	bool getProgress();



};



