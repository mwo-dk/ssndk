module SsnDk(ErrorReason(..), allInts) where

import Data.Char
import Data.Time.Calendar

-- Represents an error reason
data ErrorReason =
    NullEmptyOrWhiteSpace               -- The value was null, empty or white space
    | InvalidInput                      -- Generic invalid input
    | NonDigitCharacters                -- The value contained non-digit characters, where characters were expected
    | NonDashCharacter                  -- The value contained a non-dash character where a dash was expected
    | Modula11CheckFail                 -- The modula 11 check failed
    | InvalidLength                     -- The trimmed range has invalid length
    | InvalidDayInMonth                 -- The given day in the month is invalid
    | InvalidMonth                      -- The given month is invalid
    | InvalidYear                       -- The year is invalid
    | InvalidControl                    -- The control number is invalid
    | InvalidYearAndControl             -- The year and control numbers are invalid
    | InvalidYearAndControlCombination  -- Essential unexpected error
    deriving (Show)

dash = '-'

isDash :: Char -> Bool
isDash x = x == dash

isSpaceOrDash :: Char -> Bool
isSpaceOrDash x = isSpace x || isDash x

data CursorPair = Unknown | Known (Int, Int)
    deriving(Show)

getRange :: Int -> CursorPair -> [Char] -> CursorPair
getRange _ cursors [] = cursors
getRange n cursors [x] =
    case cursors of
        Known (first, _) -> 
            if isSpace x then cursors
            else Known (first, n)
        _ -> Unknown
getRange n cursors (x:xs) =
    if isSpace x then getRange (n+1) cursors xs
    else 
        case cursors of
            Known (first, _) -> getRange (n+1) (Known(first, n)) xs
            _ -> getRange (n+1) (Known(n, n)) xs

getIndices :: [Char] -> CursorPair
getIndices x = getRange 0 Unknown x

allInts' :: Bool -> Int -> Int -> [Char] -> Bool
allInts' False _ _ _ = False
allInts' acc _ _ [] = acc
allInts' acc 0 0 _ = acc
allInts' acc 0 _ [x] = acc && (isDigit x)
allInts' acc 0 last (x:xs) = acc && (isDigit x) && (allInts' acc 0 (last-1) xs)
allInts' acc first last (x:xs) = acc && (allInts' acc (first-1) (last-1) xs)

allInts :: Int -> Int -> [Char] -> Bool
allInts first last x = allInts' True first last x

weights = [4, 3, 2, 7, 6, 5, 4, 3, 2, 1]

data SumOfProduct = SumOfProductResult Int | NoSumOfProductResult
    deriving(Show)

addAndMul :: SumOfProduct -> Char -> Int -> SumOfProduct
addAndMul acc c x = 
    if isDigit c then
        case acc of
            NoSumOfProductResult -> SumOfProductResult ((digitToInt c)*x)
            SumOfProductResult acc' -> SumOfProductResult (acc' + (digitToInt c)*x)
    else acc

sumOfProduct' :: SumOfProduct -> [Char] -> [Int] -> SumOfProduct
sumOfProduct' acc [] _ = acc
sumOfProduct' acc _ [] = acc
sumOfProduct' acc [x] [w] = addAndMul acc x w
sumOfProduct' acc [x] (w:ws) = addAndMul acc x w
sumOfProduct' acc (x:xs) [w] = 
    if isSpaceOrDash x then sumOfProduct' acc xs [w]
    else addAndMul acc x w
sumOfProduct' acc (x:xs) (w:ws) = 
    if isSpaceOrDash x then sumOfProduct' acc xs ([w] ++ ws)
    else sumOfProduct' (addAndMul acc x w) xs ws

sumOfProduct :: [Char] -> SumOfProduct
sumOfProduct x = sumOfProduct' NoSumOfProductResult x weights

modula11Of :: Int -> Int
modula11Of x = x `mod` 11

isModula11 :: Int -> Bool
isModula11 x = 0 == modula11Of x

data IntValue = Value Int | NoValue
    deriving(Show)

getNthDigit :: Int -> [Char] -> IntValue
getNthDigit n x =
    case (n, x) of
        (_, []) -> NoValue
        (0, [d]) -> 
            if isDigit d then Value (digitToInt d)
            else NoValue
        (0, (d:ds)) -> 
            if isDigit d then Value (digitToInt d)
            else getNthDigit 0 ds
        (n, [d]) -> NoValue
        (n, (d:ds)) -> 
            if isDigit d then getNthDigit (n-1) ds
            else getNthDigit n ds

getDD :: [Char] -> IntValue
getDD x =
    case (getNthDigit 0 x, getNthDigit 1 x) of
        (Value a, Value b) -> Value (10*a+b)
        _ -> NoValue

getMM :: [Char] -> IntValue
getMM x =
    case (getNthDigit 2 x, getNthDigit 3 x) of
        (Value a, Value b) -> Value (10*a+b)
        _ -> NoValue

getYY :: [Char] -> IntValue
getYY x =
    case (getNthDigit 4 x, getNthDigit 5 x) of
        (Value a, Value b) -> Value (10*a+b)
        _ -> NoValue

getControlCode :: [Char] -> IntValue
getControlCode x =
    case (getNthDigit 6 x, getNthDigit 7 x, getNthDigit 8 x, getNthDigit 9 x) of
        (Value a, Value b, Value c, Value d) -> Value (1000*a+100*b+10*c+d)
        _ -> NoValue

inRange :: Int -> Int -> Int -> Bool
inRange a b x = a <= x && x <= b

getBirthYear' :: Int -> Int -> Int
getBirthYear' yy controlCode 
                                | inRange 0 99 yy && inRange 0 3999 controlCode = 1900 + yy
                                | inRange 0 36 yy && inRange 4000 4999 controlCode = 2000 + yy
                                | inRange 37 99 yy && inRange 4000 49999 controlCode = 1900 + yy
                                | inRange 0 57 yy && inRange 5000 8999 controlCode = 2000 + yy
                                | inRange 58 99 yy && inRange 5000 8999 controlCode = 1800 + yy
                                | inRange 0 36 yy && inRange 9000 9999 controlCode = 2000 + yy
                                | inRange 37 99 yy && inRange 9000 9999 controlCode = 1900 + yy

data YearOfBirth = YearOfBirthSuccess Int | YearOfBirthError ErrorReason   
    deriving(Show)

getBirthYear :: Int -> Int -> YearOfBirth
getBirthYear yy controlCode =
    case (inRange 0 99 yy, inRange 0 9999 controlCode) of
        (False, True) -> YearOfBirthError InvalidYear
        (True, False) -> YearOfBirthError InvalidControl
        (False, False) -> YearOfBirthError InvalidYearAndControl
        (True, True) -> YearOfBirthSuccess (getBirthYear' yy controlCode)

data Gender = Male | Female
    deriving(Show)

getGender :: Int -> Gender
getGender x = if x `mod` 2 == 0 then Female else Male

validateDigitsAndDash :: Int -> Int -> [Char] -> (Bool, Bool)
validateDigitsAndDash first last ssn =
    case last - first + 1 of
        10 -> (allInts first last ssn, True)
        11 -> (allInts first (first + 5) ssn && allInts (first + 7) last ssn, isDash (ssn!!6))
        _ -> (False, False)

data Validation = ValidationSuccess (Int, Int, Int, Gender) | ValidationError ErrorReason
    deriving(Show)

isMonthValid :: Int -> Bool
isMonthValid mm = inRange 1 12 mm

repairDayInMonth' :: Bool -> IntValue -> IntValue
repairDayInMonth' repairDayInMonth dd =
    case dd of 
        Value dd -> if 61 <= dd && repairDayInMonth then Value (dd - 60) else Value dd
        _ -> NoValue

isDayInMonthValid :: Int -> Int -> Int -> Bool
isDayInMonthValid year mm dd =
    inRange 1 (gregorianMonthLength (toInteger year) dd) dd


data DDMMYYCC = DDMMYYCC (Int, Int, Int, Int) | NoDDMMYYCC
getDDMMYYCC :: Int -> Bool  -> [Char] -> DDMMYYCC
getDDMMYYCC first repairDayInMonth ssn =
    case (repairDayInMonth' repairDayInMonth (getDD ssn), getMM ssn, getYY ssn, getControlCode ssn) of
        (Value dd, Value mm, Value yy, Value cc) -> DDMMYYCC(dd, mm, yy, cc)
        _ -> NoDDMMYYCC

getDDMMYYGender :: Int -> Bool  -> [Char] -> Validation
getDDMMYYGender first repairDayInMonth ssn =
    case (getDDMMYYCC first repairDayInMonth ssn) of 
        DDMMYYCC(dd, mm, yy, controlCode) ->
            if not (isMonthValid dd) then ValidationError InvalidMonth
            else 
                case getBirthYear yy controlCode of
                    YearOfBirthSuccess year -> 
                        if not(isDayInMonthValid year mm dd) then ValidationError InvalidDayInMonth
                        else ValidationSuccess (dd, mm, year, getGender controlCode)
                    YearOfBirthError reason -> ValidationError reason
        _ -> ValidationError InvalidInput

validate' :: Bool -> Bool -> [Char] -> Validation
validate' useModula11Check repairDayInMonth ssn =
    case getIndices ssn of
        Known (first, last) ->
            case validateDigitsAndDash first last ssn of
                (False, _) -> ValidationError NonDigitCharacters
                (_, False) -> ValidationError NonDashCharacter
                _ ->
                    case getDDMMYYGender first repairDayInMonth ssn of
                        ValidationSuccess (dd, mm, yy, gender) ->
                            if useModula11Check then
                                case sumOfProduct ssn of
                                    SumOfProductResult value -> 
                                        if isModula11 value then ValidationSuccess (dd, mm, yy, gender)
                                        else ValidationError Modula11CheckFail
                                    NoSumOfProductResult -> ValidationError Modula11CheckFail
                            else ValidationSuccess (dd, mm, yy, gender)
                        ValidationError reason -> ValidationError reason
        _ -> ValidationError NullEmptyOrWhiteSpace

data ValidationResult = Valid | Invalid ErrorReason
    deriving(Show)

validate :: Bool -> Bool -> [Char] -> ValidationResult
validate useModula11Check repairDayInMonth ssn =
    case validate' useModula11Check repairDayInMonth ssn of
        ValidationSuccess _ -> Valid
        ValidationError reason -> Invalid reason

data PersonInfo = PersonInfo {
    gender :: Gender,
    dateOfBirth :: Day
} deriving(Show)

data SSNResult = Success PersonInfo | Failure ErrorReason

getPersonInfo :: Bool -> Bool -> [Char] -> SSNResult
getPersonInfo useModula11Check repairDayInMonth ssn =
    case validate' useModula11Check repairDayInMonth ssn of
        ValidationSuccess (dd, mm, year, gender) ->
            Success PersonInfo {gender = gender, dateOfBirth = fromGregorian (toInteger year) mm dd}
        ValidationError reason -> Failure reason