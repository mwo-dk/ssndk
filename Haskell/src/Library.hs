module SsnDk(ErrorReason(..), allInts) where

import Data.Char
import Data.Time
import Data.Time.Calendar

-- Represents an error reason
data ErrorReason =
    NullEmptyOrWhiteSpace               -- The value was null, empty or white space
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

getIndices' :: Int -> Maybe Int -> Maybe Int -> [Char] -> (Maybe Int, Maybe Int)
getIndices' n first last [] = (first, last)
getIndices' n first last (x:xs) =
    if x == ' ' then
        getIndices' (n+1) first last xs
    else 
        if first == Nothing then getIndices' (n+1) (Just n) (Just n) xs
        else getIndices' (n+1) first (Just n) xs

data IndexRange = Range (Int, Int) | NoRamge
    deriving (Show)

getIndices :: [Char] -> IndexRange
getIndices x = 
    case getIndices' 0 Nothing Nothing x of
        (Just first, Just last) -> Range (first, last)
        (_, _) -> NoRange


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
    if x == ' ' || x == '-' then sumOfProduct' acc xs [w]
    else addAndMul acc x w
sumOfProduct' acc (x:xs) (w:ws) = 
    if x == ' ' || x == '-' then sumOfProduct' acc xs ([w] ++ ws)
    else sumOfProduct' (addAndMul acc x w) xs ws

sumOfProduct :: [Char] -> SumOfProduct
sumOfProduct x = sumOfProduct' NoResult x weights

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

data BirthYear = BirthYearSuccess Int | BirthYearError ErrorReason                                
getBirthYear :: Int -> Int -> BirthYear
getBirthYear yy controlCode =
    case (inRange 0 99 yy, inRange 0 9999 controlCode) of
        (True, True) -> BirthYearSuccess (getBirthYear' yy controlCode)
        (False, True) -> BirthYearError InvalidYear
        (True, False) -> BirthYearError InvalidControl
        (False, False) -> BirthYearError InvalidYearAndControl

data Gender = Male | Female
    deriving(Show)

getGender :: Int -> Gender
getGender x = if x `mod` 2 == 0 then Female else Male

data ValidationOutCome = ValidationOutComeSuccess (Int, Int, Int, Int, Gender) | ValidationOutComeError ErrorReason
    deriving(Show)

validate' :: Bool -> [Char] -> ValidationOutCome 
validate' useModula11Check ssn =
    case getIndices ssn of
        Range (first, last) ->
            let length = last - first + 1
                in
                    if length == 10 || length == 11 then
                        if not (allInts first last ssn) then ValidationOutComeError NonDigitCharacters
                        else
                            case (getDD ssn, getMM ssn, getYY ssn, getControlCode ssn) of
                                (Value dd, Value mm, Value yy, Value controlCode) ->
                                    if mm < 1 || 12 < mm then ValidationOutComeError InvalidMonth
                                    else 
                                        if useModula11Check then
                                            case sumOfProductOf ssn of
                                                SumOfProductResult n -> 
                                                    if not(isModula11 n) then ValidationOutComeError Modula11CheckFail
                                                    else ValidationOutComeSuccess (dd, mm, yy, controlCode, getGender controlCode)
                                                _ -> ValidationOutComeError Modula11CheckFail
                                        else ValidationOutComeSuccess (dd, mm, yy, controlCode, getGender controlCode)
                    else  ValidationOutComeError InvalidLength
        _ -> ValidationOutComeError NullEmptyOrWhiteSpace

data ValidationResult = ValidationResultSuccess | ValidationResultError ErrorReason
    deriving(Show)

validate :: Bool -> [Char] -> ValidationResult
validate useModula11Check ssn =
    case validate' useModula11Check ssn of
        ValidationOutComeSuccess _ -> ValidationResultSuccess
        ValidationOutComeError reason -> ValidationResultError reason

data Person = Person (Gender, DateTime)
    deriving(Show)

data SSNResult = SSNResultSuccess Person | SSNResultError ErrorReason

getPersonInfo :: Bool -> Bool -> [Char] -> SSNResult
getPersonInfo useModula11Check repairDateOfBirth ssn =
    case validate' useModula11Check ssn of
        ValidationOutComeSuccess (dd, mm, yy, controlCode, gender) ->
            case getBirthYear yy controlCode of
                BirthYearSuccess year ->
                    if 31 < dd && not(repairDateOfBirth) then SSNResultError InvalidDayInMonth
                    else
                        if gregorianMonthLength yy mm < (if dd <= 31 then dd else dd-60) then SSNResultError InvalidDayInMonth
                        else SSNResultSuccess Person(gender, DateTime(year, if dd <= 31 then dd else dd-60, mm))
                BirthYearError reason -> SSNResultError reason
        ValidationOutComeError reason -> SSNResultError reason