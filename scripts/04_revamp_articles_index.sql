DROP INDEX articles_index;

CREATE INDEX articles_index
    ON orleansstorage USING btree ((payloadjson ->> 'CreatedAt'::text) desc)
    INCLUDE (grainidn1, grainidextensionstring)
    WHERE ((graintypestring)::text = 'Grains.Articles.ArticleGrain,Grains.UserGrain'::text);


DROP INDEX user_details;
CREATE UNIQUE INDEX user_details ON orleansstorage ((payloadjson->>'Email')) 
WHERE graintypestring = 'Grains.Users.UserGrain,Grains.UserGrain';