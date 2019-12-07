SET Identity_insert ProducerSet ON
insert into ProducerSet
(Id, Name)
values
(1, N'Джеймс Кэмерон')
GO
SET Identity_insert ProducerSet OFF


SET Identity_insert FilmSet ON
insert into FilmSet
(id, KinopoiskId, [Name], [year], [Description], [Rating], Poster, Slogan, Link, Producer_Id)
values
(1, 
'terminator-2-sudnyy-den-1991-444',
N'Терминатор 2: Судный день',
1991, 
N'Прошло более десяти лет с тех пор, как киборг из 2029 года пытался уничтожить Сару Коннор — женщину, чей будущий сын выиграет войну человечества против машин.

Теперь у Сары родился сын Джон и время, когда он поведёт за собой выживших людей на борьбу с машинами, неумолимо приближается. Именно в этот момент из постапокалиптического будущего прибывает новый терминатор — практически неуязвимая модель T-1000, способная принимать любое обличье. Цель нового терминатора уже не Сара, а уничтожение молодого Джона Коннора.

Однако шансы Джона на спасение существенно повышаются, когда на помощь приходит перепрограммированный сопротивлением терминатор предыдущего поколения. Оба киборга вступают в смертельный бой, от исхода которого зависит судьба человечества.',
8.307,
'https://www.kinopoisk.ru/images/film_big/444.jpg',
N'Same Make. Same Model. New Mission',
'https://www.kinopoisk.ru/film/terminator-2-sudnyy-den-1991-444/',
1)
SET Identity_insert FilmSet Off
GO


SET Identity_insert CountrySet ON
insert into CountrySet
(Id, Name)
Values
(1, N'США')
SET Identity_insert CountrySet Off
GO

SET Identity_insert CountryFilmSet ON
insert into CountryFilmSet
(Id, Film_Id, Country_Id)
Values
(1, 1, 1)
SET Identity_insert CountryFilmSet Off
GO


SET Identity_insert ActorSet ON
insert into ActorSet
(Id, Name)
Values
(1, N'Арнольд Шварценеггер'),
(2, N'Линда Хэмилтон'),
(3, N'Эдвард Ферлонг'),
(4, N'Роберт Патрик')
SET Identity_insert ActorSet Off
GO

SET Identity_insert ActorFilmSet ON
insert into ActorFilmSet
(Id, Film_Id, Actor_Id)
Values
(1, 1, 1),
(2, 1, 2),
(3, 1, 3),
(4, 1, 4)
SET Identity_insert ActorFilmSet Off
GO

SET Identity_insert GenreSet ON
insert into GenreSet
(Id, Name)
values
(1, N'фантастика'),
(2, N'боевик'),
(3, N'триллер')
SET Identity_insert GenreSet Off
GO

SET Identity_insert GenreFilmSet ON
insert into GenreFilmSet
(Id, Film_Id, Genre_Id)
values
(1, 1, 1),
(2, 1, 2),
(3, 1, 3)
SET Identity_insert GenreFilmSet Off
GO














