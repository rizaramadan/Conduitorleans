DROP INDEX articles_index;

CREATE INDEX articles_index
    ON orleansstorage USING btree ((payloadjson ->> 'CreatedAt'::text) desc)
    INCLUDE (grainidn1, grainidextensionstring)
    WHERE ((graintypestring)::text = 'Grains.Articles.ArticleGrain,Grains.UserGrain'::text);