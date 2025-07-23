#include "Option.h"
#include "Question.h"
#include "Room.h"

//============NEEDS COMMENTS===================

Option::Option(string text) {
	this->text = text;
}

void Option::setNextQIndex(int index) {

	nextQIndex = index;

}
Option::Option() {

}

void Option::setProgress(bool progress) {
	this->progress = progress;

}

bool Option::getProgress() {
	return progress;
}

string Option::toString() const {
	return text;
}

int Option::getNextQ() {
	return nextQIndex;
}

string Option::getText() const
{
	return text;
}

int Option::getNextQIndex() const
{
	return nextQIndex;
}

