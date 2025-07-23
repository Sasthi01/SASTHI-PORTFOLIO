#pragma once
using namespace std;
#include <speechapi_cxx.h>
#include <iostream>

class TextToSpeech
{

private:
	std::shared_ptr<Microsoft::CognitiveServices::Speech::SpeechConfig> config;

public:
	TextToSpeech();
	void speak(string text);
};
