select RussianTitle, Year, RussianDescription, Rating, RussianGenries, RussianTags, EnglishActors, EnglishCountries, EnglishProducers, Poster, IMDbId
from IMDbLoadingSet
where RussianTitle is not null
and RussianDescription is not null
and Rating > '5.0'
and RussianGenries is not null
order by Rating desc  -- 21690
 
 

select * from filmset
order by name

-- 17-39 9653
