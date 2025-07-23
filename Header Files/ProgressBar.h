#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Window.hpp>

// Class to create progress bar within each room and keeps track of total percentage completed of the game

class ProgressBar {
private:
	int incrementSize; // Size of each increment
	float progress; // How far along the bar is
	float maxWidth; // The full length of the bar
	// Two rectangles for progress bar: drawn as an outline being gradually filled
	sf::RectangleShape progressBarOutline;
	sf::RectangleShape progressBarFill;

	// Progress variable to provide final feedback to the player 
	static int answeredQuestions; // The total number of progression questions the player answers


public:
	ProgressBar(int numQuestions); // Constructor
	void draw(sf::RenderWindow& window); // Creates progress bar
	void update(); // Increments the progress bar
	void decrease();//decrease the progress bar
	static void resetAnsweredQuestions();
	static void updateAnsweredQuestions(int increment); // Method to increment the number of questions answered
	static int getAnsweredQuestions(); // Returns the percentage of the game completed
	static void decreaseAnsweredQuestions(int increment); // Method to decrease the number of questions answered
	sf::RectangleShape& getOutline() { return progressBarOutline; }
	sf::RectangleShape& getFill() { return progressBarFill; }

};
