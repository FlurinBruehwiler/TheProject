# Database

We need some kind of construct on top of a LMDB transaction, we will call it session. There can't be multiple write LMDB transactions at the same time, 
but we need it. The idea is to have a separate LMDB DB that stores the dataset of this session. When querring the session, 
we first query the dataset and if the value is not in the dataset we query the main database. 
Once we want to commit a session, we open a write transaction onto the db and write in all the changes of the dataset. 
In the future, when we have multiple replicas of the database, we also send this changeset across the wire. 