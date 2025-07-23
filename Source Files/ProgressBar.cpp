#include "ProgressBar.h"
#include "Room.h"
#include<iostream>

using namespace std;
//Initialise answeredQuestions variable from Progress Bar class
int ProgressBar::answeredQuestions = 0;

ProgressBar::ProgressBar(int numQuestions) {
    // Draws the progress bar

    // Rectangle Progress Bar Outline (Transparent with Black Outline)
    progressBarOutline.setSize(sf::Vector2f(200.f, 15.f)); // Width: 200, Height: 15
    progressBarOutline.setFillColor(sf::Color::Transparent);
    progressBarOutline.setOutlineThickness(3.f);
    progressBarOutline.setOutlineColor(sf::Color::White);
    progressBarOutline.setPosition({ 545.f, 50.f }); // Centred at the top

    // Progress Bar Fill (Solid Green and starts from 0)
    progressBarFill.setSize(sf::Vector2f(0.f, 15.f)); // Start at width 0
    progressBarFill.setFillColor(sf::Color::Green);
    progressBarFill.setPosition({ 545.f, 50.f }); // Same position as the outline

    // Progress variables 
    this->progress = 0.f;
    this->maxWidth = 200.f; // The full width of the progress bar	
    if (numQuestions > 0)
        this->incrementSize = 200.f / numQuestions;

}

void ProgressBar::updateAnsweredQuestions(int increment) {
    answeredQuestions += increment;
}


int ProgressBar::getAnsweredQuestions() {
    return answeredQuestions;
}

void ProgressBar::draw(sf::RenderWindow& window) {
    window.draw(progressBarOutline);
    window.draw(progressBarFill);
}

// Function to increment progress bar
void ProgressBar::update() {
    if (progress < maxWidth) {
        progress += incrementSize;  // Increment progress
        if (progress > maxWidth) {
            progress = maxWidth; // Cap at max width
        }
    }

    // Update progress bar fill width
    progressBarFill.setSize(sf::Vector2f(progress, 15.f));
}

void ProgressBar::decrease() {
    if (progress <= maxWidth) {
        progress -= incrementSize;  // Increment progress
    }

    // Update progress bar fill width
    progressBarFill.setSize(sf::Vector2f(progress, 15.f));
}

void ProgressBar::resetAnsweredQuestions() {
    answeredQuestions = 0;
}

void ProgressBar::decreaseAnsweredQuestions(int increment) {//removes answered question if you don't have item
    answeredQuestions -= increment;
}
