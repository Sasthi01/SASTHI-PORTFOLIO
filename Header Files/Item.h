#pragma once
#include <string>
#include <SFML/Graphics.hpp>

using namespace std;
class Item
{

private:

	string itemName;
	string imagePath;
	sf::Texture texture; // Texture for the item

public:
	void showItem();
	bool operator==(const Item& i) const; //Operator overloading to check if two items are equal
	Item(string itemName,string imagePath);
	Item(string itemName); // Add this constructor to handle single argument  NEW
	void setImagePath(string image);
	std::string getName() const;
	// New method to get the texture
	const sf::Texture& getTexture() const;
};



