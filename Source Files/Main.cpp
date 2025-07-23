#include <iostream>
#include "Game.h"
#include <speechapi_cxx.h>

using namespace Microsoft::CognitiveServices::Speech;


int main()
{

	
	//////////////////////////
	

	//Initialize Game Engine
	Game game;

	//Game loop
	while (game.getRunning()) 
	{
		//Update
		game.update();
		//Render
		game.render();
	}
	return 0;
}
