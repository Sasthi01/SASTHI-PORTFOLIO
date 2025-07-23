#include "Room.h"
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <iostream>
#include <algorithm>

/*Parent class for rooms*/

using namespace std;
class Room;

//NEW
Room::Room() {
	itemRectangle.setSize({ 50.f, 50.f }); // Small rectangle
	itemRectangle.setTexture(&itemTexture);
	itemRectangle.setPosition({ 600.f, 300.f }); // Initial position
}

//==========NEEDS COMMENTS==============
Room::Room(string roomName) {
	this->roomName = roomName;
	initializeVariables();

}

void Room::initializeVariables() {
	currentIndex = 1;
	extractQuestions();
}

//Extracts the questions from the textfile
void Room::extractQuestions() {

	ifstream roomFile; //Create file
	roomFile.open("Assets/Textfiles/" + roomName + ".txt"); //Open textfile
	string line;

	//Reads the contents from the textfile line by line. Repeat until reaches the end of the textfile. 
	while (getline(roomFile, line)) {

		//the textfile reads the "\" and "n" separately so the skip line isnt registered, so we find the \n in the textfile and replace it
		int posSpace;
		while ((posSpace = line.find("\\n")) != std::string::npos) {
			line.replace(posSpace, 2, "\n");
		}
		//Read Question
		if (line == "") {
			continue;
		}

		char del = '#';

		int pos = line.find(del);

		Question q(line.substr(0, pos));
		int key = stoi(line.substr(pos + 1, line.length() - pos - 1));
		getline(roomFile, line);

		int numOptions = stoi(line);
		q.setNumOptions(numOptions);


		//Read options
		for (int i = 0; i < numOptions; i++) {
			getline(roomFile, line);



			pos = line.find(del);
			Option o(line.substr(0, pos));

			char delProgress = '$';
			int posProgress = line.find(delProgress);

			int nextQ;

			if (posProgress == -1) {
				nextQ = stoi(line.substr(pos + 1, line.size() - (pos - 1)));

				o.setProgress(false);
			}
			else {
				
				nextQ = stoi(line.substr(pos + 1, line.size() - (pos + 2)));
				o.setProgress(true);

			}


			o.setNextQIndex(nextQ);


			q.addOption(o);
		}




		addQuestion(q, key);


	}


	roomFile.close(); //Close the textfile


}

void Room::setRoomQuestions(int numQuestions) {
	this->roomQuestions = numQuestions;
}

int Room::getRoomQuestions() {
	return roomQuestions;
}


void Room::addQuestion(Question q, int key) {
	questions.emplace(key, q);

}

void Room::handleItemClick(sf::Vector2i& mousePos) {
	// Handle clicks on the item rectangle
	if (itemRectangle.getGlobalBounds().contains(sf::Vector2f(mousePos))) {
		itemRectangle.setPosition({ 50.f, itemRectangle.getPosition().y }); // Move to the left
	}
}

void Room::renderItem(sf::RenderWindow& window) {
	// Draw the item rectangle
	window.draw(itemRectangle);
}


void Room::loadChangePos(images& image)
{
	ifstream imageFile; //Create file
	imageFile.open("Assets/Textfiles/" + image.name + ".txt"); //Open textfile
	int pos;

	while (imageFile >> pos) {
		image.changePos.push_back(pos);
	}

	imageFile.close();
}

// Sets the name of the room
void Room::setRoomName(string name) {
	roomName = name;
}

//Returns the name of the room
string Room::getRoomName() {
	return roomName;
}

void Room::addItem(Item i) {

}
void Room::pickUp(Item i) {

}
void Room::exitRoom() {
	
}
void Room::enterRoom() {

}

//Set backround image
void Room::setBackground(const std::string& roomPath)
{
	background.loadFromFile("Assets/BackgroundImages/" + roomPath + ".jpg");
	backgroundImage.setTexture(background);

}

void Room::loadImages(const std::string& imagePath, int num)
{
	for (int i = 1; i <= num; i++) {
		images newImage;
		newImage.name = imagePath + std::to_string(i);
		loadChangePos(newImage); //loads the curentIndex values that result in the background image changing to specific image.
		backgroundImages.push_back(newImage);
	}
}


//=============NEEDS COMMENTS================== 

void Room::check() {

}

void Room::checkImages()
{
	for (images i : backgroundImages) {
		for (int num : i.changePos) {
			if (currentIndex == num) {
				setBackground(i.name);
			}
		}
	}
}

Question Room::getCurrentQuestion() {
	return questions[currentIndex];
}

void Room::moveToNextQuestion(int nextIndex) {
	prevPrevIndex = prevIndex;
	prevIndex = currentIndex;
	currentIndex = nextIndex;
}



