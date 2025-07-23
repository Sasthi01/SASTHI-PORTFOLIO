#include "TextToSpeech.h"

#include <speechapi_cxx.h>
#include <iostream>


using namespace Microsoft::CognitiveServices::Speech;


TextToSpeech::TextToSpeech() {

    
    //Adding credentials to api
    config = SpeechConfig::FromSubscription("//key inserted here", "southafricanorth");
    
    //Sets the voice
    config->SetSpeechSynthesisVoiceName("en-US-ChristopherMultilingualNeural"); // Optional: neural voice

}

//This method will convert the entered string to speech. Call this method when you want it to narrate
void TextToSpeech::speak(string text) {

    auto synthesizer = SpeechSynthesizer::FromConfig(config);
    auto result = synthesizer->SpeakTextAsync(text).get();



}


