import Html exposing (..)
import Html.Attributes exposing (..)
import WebSocket
import Http
import Json.Decode as Decode
import Array
import Random

main =
  Html.program
    { init = init
    , view = view
    , update = update
    , subscriptions = subscriptions
    }

relayServerUrl : String
relayServerUrl =
  "ws://<>:8080"

contentApiUrl : String
contentApiUrl =
  "http://localhost:5000"
  
loadCards: Cmd Msg
loadCards = 
    Http.send CardsLoaded (Http.get (contentApiUrl ++ "/api/v1/Content/animals-home") decodeStringArray)

decodeStringArray : Decode.Decoder (Array.Array String)
decodeStringArray =
  Decode.array Decode.string

-- MODEL

type alias Match =
  { imageUrl: String
   ,cardId: String
  }

type alias Model =
  { currentMatch: Match
    , isMatched: Bool
    , currentGame: String
    , cards: Array.Array String
  }


init : (Model, Cmd Msg)
init =
  (Model (Match "" "") False "animals-home" Array.empty, loadCards)
-- UPDATE


type Msg
  = NewMessage String
  | CardsLoaded (Result Http.Error (Array.Array String))
  | SelectMatch Int


update : Msg -> Model -> (Model, Cmd Msg)
update msg model=
  case msg of
    NewMessage str ->
      let 
        isMatch = 
            str == model.currentMatch.cardId
      in
        ( {model | isMatched = isMatch}, Cmd.none)
    CardsLoaded (Ok res)  ->
        ( {model | cards = res }, Random.generate SelectMatch (Random.int 0 (Array.length res - 1)))
    CardsLoaded (Err _) ->
        ( {model | currentMatch = (Match "" "") }, Cmd.none)
    SelectMatch id ->
        let 
            currentCard: String
            currentCard = Maybe.withDefault "not-found" (Array.get id model.cards)
            getImageUrl = contentApiUrl ++ "/api/v1/Content/animals-home/" ++ currentCard
            getCardId = Maybe.withDefault "not-found" (List.head (String.split "." currentCard))
        in
            ({model | currentMatch = (Match getImageUrl getCardId)}, Cmd.none)


-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
  WebSocket.listen relayServerUrl NewMessage



-- VIEW


view : Model -> Html Msg
view model =
  div []
    [ 
        p [] [text (model.currentGame)]
        , img [src model.currentMatch.imageUrl, height 100, width 100] []
        ,p [] [text (toString model.isMatched)]
    ]


viewMessage : String -> Html msg
viewMessage msg =
  div [] [ text msg ]
