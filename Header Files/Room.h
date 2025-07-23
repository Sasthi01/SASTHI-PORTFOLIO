#include <string>
#include "Item.h"
#include <vector>
#include "Images.h"
#include <map>
#include  "Question.h"
#include "Inventory.h"
#include <SFML/Graphics.hpp>
#pragma once
class Game;
/*Parent class for rooms*/

using namespace std;
class Question;
class Item;

class Room
{
protected:
	sf::RectangleShape itemRectangle;
	sf::Texture itemTexture;

	//===========NEEDS COMMENTS==========
	map<int, Question> questions;
	int progress;
	int currentIndex;
	int prevIndex;
	int prevPrevIndex;

	void initializeVariables();
private:
	string roomName = "";
	string bgImagePath;
	int roomQuestions;

public:
	virtual void handleItemClick(sf::Vector2i& mousePos); // Method declaration
	virtual void renderItem(sf::RenderWindow& window);    // Method declaration

	//=============NEEDS COMMENTS=============
	void setRoomName(string name);
	string getRoomName();
	void addItem(Item i);
	void pickUp(Item i);
	void exitRoom();
	void enterRoom();
	Room(string roomName);
	void extractQuestions();
	void loadChangePos(images& image);
	
	void addQuestion(Question q, int key);
	Room();
	virtual void check();
	virtual void checkImages();
	Question getCurrentQuestion();
	void moveToNextQuestion(int nextIndex);

	void setRoomQuestions(int numQuestions); 
	int getRoomQuestions(); // Returns the number of questions for the room

	//variables
	//graphic variables for rooms
	sf::Texture background;
	sf::Sprite backgroundImage;
	//vector of image structs
	vector<images> backgroundImages;

	//Functions
	// Virtual function to set background, with roomPath as parameter
	virtual void setBackground(const std::string& roomPath);
	//virtual function to load images into vector
	virtual void loadImages(const std::string& imagePath, int num);

	//Virtual functions for child classes to override
	virtual void handleInput(sf::Vector2i& mousePos) = 0;
	virtual void update() = 0;
	virtual void render() = 0;




};




