#include "Item.h"

Item::Item(string itemName, string imagePath) {
	this->itemName = itemName;
	this->imagePath = imagePath;
	if (!texture.loadFromFile(imagePath)) {
		throw std::runtime_error("Failed to load texture from: " + imagePath);
	}
}

 // Add implementation for the new constructor  
    Item::Item(string itemName) : itemName(itemName), imagePath("") {}  

void Item::setImagePath(string imagePath) {
	this->imagePath = imagePath;
	if (!texture.loadFromFile(imagePath)) {
		throw std::runtime_error("Failed to load texture from: " + imagePath);
	}
}

string Item::getName() const {
	return itemName;
}

const sf::Texture& Item::getTexture() const {
	return texture;
}

void Item::showItem() {

}

//Operator overloading needed in the find method to check if an item is in the inventory
//This definition is required the find method will not work without it

bool Item::operator == (const Item& i) const{

	if (itemName == i.itemName) {
		return true;
	}
	else {
		return false;
	}

}

