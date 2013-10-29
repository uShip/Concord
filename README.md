Concord
=======

A parallel unit test runner, with poor documentation.

### Craziness:
You can do: -categories@=IAMAFILEPATH.txt  to run only specific categories
(This does not support feature names)

### TODO:
* ~~Bug when using -uncategorizedInParallel: running some that have category names...~~
* If a full feature is ignored, do not include it in the run
  * I don't believe I can do this with categories though, becauase a category might have two features, where one is ignored and the other isn't
