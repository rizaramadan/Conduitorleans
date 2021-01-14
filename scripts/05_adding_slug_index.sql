CREATE INDEX slug_index
    ON orleansstorage using btree ((payloadjson->>'Slug'))
    INCLUDE (grainidn1, grainidextensionstring)
    WHERE graintypestring = 'Grains.Articles.ArticleGrain,Grains.ArticleGrain';