// Day 5-6: Data Structures - Practice Tasks Implementation

printfn "============ Day 5-6: Data Structures Practice Tasks ============"

printfn "============ Task 1: Library Management System ============"

// Task 1: Library Management System Types
type Book =
    { Title: string
      Author: string
      ISBN: string
      Genre: string
      Available: bool }

type MembershipType =
    | Basic
    | Premium
    | Student

type Member =
    { ID: int
      Name: string
      Email: string
      MembershipType: MembershipType }

type LoanStatus =
    | Active
    | Returned
    | Overdue

type Loan =
    { Book: Book
      Member: Member
      LoanDate: System.DateTime
      DueDate: System.DateTime
      Status: LoanStatus }

// Task 1: Library functions
let checkAvailability (book: Book) = book.Available

let calculateFine (loan: Loan) =
    if loan.Status = Overdue then
        let overdueDays = (System.DateTime.Now - loan.DueDate).Days
        printfn "Calculating fine for %d overdue days." overdueDays
        let finePerDay =
            match loan.Member.MembershipType with
            | Basic -> 2.0
            | Premium -> 1.0
            | Student -> 0.5

        float overdueDays * finePerDay
    else
        0.0

let borrowBook (book: Book) (memberData: Member) =
    if checkAvailability book then
        let loanPeriod =
            match memberData.MembershipType with
            | Basic -> 14
            | Premium -> 21
            | Student -> 30
        printfn "📚 Book '%s' borrowed by %s for %d days." book.Title memberData.Name loanPeriod
        let loan =
            { Book = { book with Available = false }
              Member = memberData
              LoanDate = System.DateTime.Now
              DueDate = System.DateTime.Now.AddDays(float loanPeriod)
              Status = Active }

        Some loan
    else
        None

// Test Library Management System
let book1 =
    { Title = "F# for Fun and Profit"
      Author = "Scott Wlaschin"
      ISBN = "978-1234567890"
      Genre = "Programming"
      Available = true }

let book2 =
    { Title = "F# for Fun and Profit - Second Edition"
      Author = "Scott Wlaschin"
      ISBN = "978-1234567890"
      Genre = "Programming"
      Available = false }

let member1 =
    { ID = 1
      Name = "Alice Johnson"
      Email = "alice@example.com"
      MembershipType = Premium }

printfn "============Book availability: %b" (checkAvailability book1)
match borrowBook book1 member1 with
| Some loan ->
    printfn "Book borrowed by %s, due date: %s" loan.Member.Name (loan.DueDate.ToString("yyyy-MM-dd"))
    printfn "Current fine: $%.2f" (calculateFine loan)
| None -> printfn "Book not available"

printfn "============Book not availability: %b" (checkAvailability book2)
match borrowBook book2 member1 with
| Some loan ->
    printfn "Book borrowed by %s, due date: %s" loan.Member.Name (loan.DueDate.ToString("yyyy-MM-dd"))
    let loanOverdue = { loan with Status = Overdue }
    printfn "Current fine: $%.2f" (calculateFine loanOverdue)
| None -> printfn "Book not available: %s" book2.Title

printfn "============Loan with Overdue: %b" (checkAvailability book1)
match borrowBook book1 member1 with
| Some loan ->
    let loanOverdue = { loan with Status = Overdue; DueDate = loan.DueDate.AddDays(-50.0) }
    printfn "Book borrowed by %s, due date: %s" loanOverdue.Member.Name (loanOverdue.DueDate.ToString("yyyy-MM-dd"))
    printfn "Current fine: $%.2f" (calculateFine loanOverdue)
| None -> printfn "Book not available: %s" book1.Title

printfn "\n============ Task 2: Advanced List Operations ============"

// Task 2.1: groupBy - group list elements by a key function
let groupBy keyFunction lst =
    let rec groupHelper acc remaining =
        match remaining with
        | [] -> acc |> List.map (fun (key, items) -> (key, List.rev items))
        | head :: tail ->
            let key = keyFunction head
            match List.tryFind (fun (k, _) -> k = key) acc with
            | Some (_, items) ->
                let updatedAcc = 
                    acc |> List.map (fun (k, items) -> 
                        if k = key then (k, head :: items) else (k, items))
                groupHelper updatedAcc tail
            | None ->
                groupHelper ((key, [head]) :: acc) tail
    groupHelper [] lst

// Task 2.2: partition - split list into two based on predicate
let partition predicate lst =
    let rec partitionHelper trueList falseList remaining =
        match remaining with
        | [] -> (List.rev trueList, List.rev falseList)
        | head :: tail ->
            if predicate head then
                partitionHelper (head :: trueList) falseList tail
            else
                partitionHelper trueList (head :: falseList) tail
    partitionHelper [] [] lst

// Task 2.3: zip - combine two lists into tuple list
let rec zip list1 list2 =
    match list1, list2 with
    | [], _ | _, [] -> []
    | head1 :: tail1, head2 :: tail2 ->
        (head1, head2) :: zip tail1 tail2

// Task 2.4: flatMap - map and flatten results
let flatMap mapFunction lst =
    let rec flatMapHelper acc remaining =
        match remaining with
        | [] -> List.rev acc
        | head :: tail ->
            let mapped = mapFunction head
            flatMapHelper (List.rev mapped @ acc) tail
    flatMapHelper [] lst

// Test Advanced List Operations
let numbers = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let words = ["apple"; "banana"; "apricot"; "blueberry"; "cherry"; "avocado"]

printfn "Original numbers: %A" numbers
printfn "Group by even/odd: %A" (groupBy (fun x -> x % 2 = 0) numbers)

printfn "Original words: %A" words
printfn "Group by first letter: %A" (groupBy (fun (w: string) -> w.[0]) words)

let (evens, odds) = partition (fun x -> x % 2 = 0) numbers
printfn "Partition numbers - Evens: %A, Odds: %A" evens odds

let list1 = [1; 2; 3; 4]
let list2 = ["a"; "b"; "c"; "d"]
printfn "Zip %A and %A: %A" list1 list2 (zip list1 list2)

printfn "FlatMap numbers to [x, x*2]: %A" (flatMap (fun x -> [x; x * 2]) [1; 2; 3])

printfn "\n============ Task 3: Game Character System ============"

// Task 3: Game Character System
type CharacterStats = {
    Health: int
    Mana: int
    Strength: int
    Defense: int
    Speed: int
}

type WeaponType =
    | Sword of int  // damage
    | Bow of int    // damage
    | Staff of int  // magic damage

type ArmorType =
    | LightArmor of int  // defense
    | HeavyArmor of int  // defense
    | Robe of int        // magic defense

type Equipment = {
    Weapon: WeaponType option
    Armor: ArmorType option
    Accessory: string option
}

type Skill =
    | Attack of int      // damage multiplier
    | Heal of int        // heal amount
    | Shield of int      // defense boost

type Character = {
    Name: string
    Stats: CharacterStats
    Equipment: Equipment
    Skills: Skill list
    Level: int
}

// Combat calculation functions
let getWeaponDamage weapon =
    match weapon with
    | Some (Sword damage) -> damage
    | Some (Bow damage) -> damage
    | Some (Staff damage) -> damage
    | None -> 0

let getArmorDefense armor =
    match armor with
    | Some (LightArmor def) -> def
    | Some (HeavyArmor def) -> def
    | Some (Robe def) -> def
    | None -> 0

let calculateDamage attacker defender =
    let weaponDamage = getWeaponDamage attacker.Equipment.Weapon
    let totalAttack = attacker.Stats.Strength + weaponDamage
    let totalDefense = defender.Stats.Defense + getArmorDefense defender.Equipment.Armor
    max 1 (totalAttack - totalDefense)

let useSkill character skill =
    match skill with
    | Attack multiplier ->
        let damage = character.Stats.Strength * multiplier
        printfn "%s uses Attack skill for %d damage!" character.Name damage
        character
    | Heal amount ->
        let newHealth = min 100 (character.Stats.Health + amount)
        printfn "%s heals for %d HP! New health: %d" character.Name amount newHealth
        { character with Stats = { character.Stats with Health = newHealth } }
    | Shield boost ->
        printfn "%s uses Shield skill, defense boosted by %d!" character.Name boost
        { character with Stats = { character.Stats with Defense = character.Stats.Defense + boost } }

// Test Game Character System
let warrior = {
    Name = "Warrior"
    Stats = { Health = 100; Mana = 20; Strength = 15; Defense = 10; Speed = 8 }
    Equipment = { 
        Weapon = Some (Sword 12)
        Armor = Some (HeavyArmor 8)
        Accessory = Some "Ring of Power"
    }
    Skills = [Attack 2; Shield 5]
    Level = 5
}

let mage = {
    Name = "Mage"
    Stats = { Health = 60; Mana = 80; Strength = 8; Defense = 5; Speed = 12 }
    Equipment = { 
        Weapon = Some (Staff 15)
        Armor = Some (Robe 3)
        Accessory = Some "Mana Crystal"
    }
    Skills = [Attack 3; Heal 20]
    Level = 5
}

printfn "=== Character Stats ==="
printfn "%s: Health=%d, Strength=%d, Defense=%d" 
    warrior.Name warrior.Stats.Health warrior.Stats.Strength warrior.Stats.Defense
printfn "%s: Health=%d, Strength=%d, Defense=%d" 
    mage.Name mage.Stats.Health mage.Stats.Strength mage.Stats.Defense

printfn "=== Combat Test ==="
let damageToMage = calculateDamage warrior mage
let damageToWarrior = calculateDamage mage warrior
printfn "%s attacks %s for %d damage!" warrior.Name mage.Name damageToMage
printfn "%s attacks %s for %d damage!" mage.Name warrior.Name damageToWarrior

printfn "=== Skill Usage ==="
let healedMage = useSkill mage (Heal 20)
let shieldedWarrior = useSkill warrior (Shield 5)

printfn "\n============ Task 4: Financial Portfolio ============"

// Task 4: Financial Portfolio System
type InvestmentType =
    | Stock of string * float * int        // symbol, price, shares
    | Bond of string * float * float       // issuer, face value, yieldRate
    | Cash of float                        // amount

type Portfolio = {
    Owner: string
    Investments: InvestmentType list
    CreatedDate: System.DateTime
}

type RiskLevel =
    | Low
    | Medium  
    | High
    | VeryHigh

// Performance calculation functions
let calculateInvestmentValue investment =
    match investment with
    | Stock (_, price, shares) -> price * float shares
    | Bond (_, faceValue, _) -> faceValue
    | Cash amount -> amount

let calculatePortfolioValue portfolio =
    portfolio.Investments
    |> List.map calculateInvestmentValue
    |> List.sum

let getInvestmentRisk investment =
    match investment with
    | Stock _ -> High
    | Bond _ -> Medium
    | Cash _ -> Low

let assessPortfolioRisk portfolio =
    let risks = portfolio.Investments |> List.map getInvestmentRisk
    let riskCounts = 
        risks |> List.groupBy id |> List.map (fun (risk, items) -> (risk, List.length items))
    
    let totalInvestments = List.length portfolio.Investments
    match riskCounts with
    | _ when List.exists (fun (risk, count) -> risk = High && count >= totalInvestments / 2) riskCounts -> High
    | _ when List.exists (fun (risk, count) -> risk = Medium && count >= totalInvestments / 2) riskCounts -> Medium
    | _ -> Low

let diversificationScore portfolio =
    let investmentTypes = 
        portfolio.Investments 
        |> List.map (function | Stock _ -> "Stock" | Bond _ -> "Bond" | Cash _ -> "Cash")
        |> List.distinct
        |> List.length
    float investmentTypes / 3.0 * 100.0

// Test Financial Portfolio
let myPortfolio = {
    Owner = "John Investor"
    Investments = [
        Stock ("AAPL", 150.0, 10)
        Stock ("GOOGL", 2500.0, 2)
        Bond ("US Treasury", 1000.0, 0.03)
        Bond ("Corporate Bond", 5000.0, 0.05)
        Cash 2000.0
    ]
    CreatedDate = System.DateTime.Now
}

printfn "=== Portfolio Analysis ==="
printfn "Portfolio Owner: %s" myPortfolio.Owner
printfn "Total Value: $%.2f" (calculatePortfolioValue myPortfolio)
printfn "Risk Assessment: %A" (assessPortfolioRisk myPortfolio)
printfn "Diversification Score: %.1f%%" (diversificationScore myPortfolio)

printfn "\n=== Individual Investment Values ==="
myPortfolio.Investments
|> List.iteri (fun i investment ->
    let value = calculateInvestmentValue investment
    let risk = getInvestmentRisk investment
    match investment with
    | Stock (symbol, price, shares) -> 
        printfn "%d. %s: %d shares @ $%.2f = $%.2f (Risk: %A)" (i+1) symbol shares price value risk
    | Bond (issuer, faceValue, yieldRate) -> 
        printfn "%d. %s Bond: $%.2f @ %.1f%% yield (Risk: %A)" (i+1) issuer faceValue (yieldRate*100.0) risk
    | Cash amount -> 
        printfn "%d. Cash: $%.2f (Risk: %A)" (i+1) amount risk
)
