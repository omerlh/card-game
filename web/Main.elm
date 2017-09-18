-- Read more about this program in the official Elm guide:
-- https://guide.elm-lang.org/architecture/effects/web_sockets.html

import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (..)
import WebSocket

main =
  Html.program
    { init = init
    , view = view
    , update = update
    , subscriptions = subscriptions
    }


echoServer : String
echoServer =
  "ws://<>:8080"
  
defaultUrl: String
defaultUrl = 
  "https://www.cesarsway.com/sites/newcesarsway/files/styles/large_article_preview/public/Common-dog-behaviors-explained.jpg?itok=FSzwbBoi"


-- MODEL

type alias Match =
  { imageUrl: String
   ,cardId: String
  }

type alias Model =
  { currentCard: String,
    currentMatch: Match,
    isMatched: Bool
  }


init : (Model, Cmd Msg)
init =
  (Model "" (Match defaultUrl "F5-C7-84-1C") False, Cmd.none)



-- UPDATE


type Msg
  = NewMessage String


update : Msg -> Model -> (Model, Cmd Msg)
update msg {currentCard, currentMatch, isMatched}=
  case msg of
    NewMessage str ->
      let 
        isMatch = 
            str == currentMatch.cardId
      in
        (Model str currentMatch isMatch, Cmd.none)



-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
  WebSocket.listen echoServer NewMessage



-- VIEW


view : Model -> Html Msg
view model =
  div []
    [ 
        img [src model.currentMatch.imageUrl] []
        ,p [] [text (toString model.isMatched)]
    ]


viewMessage : String -> Html msg
viewMessage msg =
  div [] [ text msg ]
