# gunshop progression
    gun shop has different guns each day
    callout points out "new" ?
    able to call the gun shop and see waht they have in stock?
    a random seller at the tents has some guns? different variety?
    we want there to be a variety of guns, but really good guns need to be rare / unlock later?
        let cost be the limiting factor for good weapons
        good weapons be rare
    
    the inventory is shuffled around, half guns stay, half are rerolled
    guns can have random stat bonuses (perks)
    player can ask to put a hold on a gun?



## guns need to be randomized from day to day.

assume N total guns at the end of day 0.
select a desired total number of guns for day 1 M from a poisson distribution

1. some guns carry over from the previous day
2. inventory is filled back up with randomly selected guns
    * allow at most 2 copies of the same gun
    * on draw, apply random perks drawn from weighted list on gun template
    * draw up to M guns

repeat for every gun vendor