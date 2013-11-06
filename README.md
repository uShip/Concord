Concord
=======

A parallel unit test runner, with poor documentation.

### Beta
* `-rerunFailedCategories` should look at the RunStats.html file, and only run categories that failed
  * Probably does not work with fixtures, (i.e. do not use along with `-uncategorizedInParallel`)
* `-namespace` will allow you to specify a root namespace to look under
  * It uses `.StartsWith(namespace)` so it will find test fixtures in all the types under that
  * If they have categories, and if those categories exist in other namespaces, **all the fixtures with those categories will be run**

### Craziness:
You can do: `-categories@=IAMAFILEPATH.txt`  to run only specific categories   
(This does not support feature names)

### TODO:
* ~~Bug when using `-uncategorizedInParallel` running some that have category names...~~
* If a full feature is ignored, do not include it in the run
  * I don't believe I can do this with categories though, becauase a category might have two features, where one is ignored and the other isn't
* Ability to say run by namespaces, or groups of fixtures... idk
* Make the general design of the runners use IOC, instead of hacked together as it is now
  * Use `RunnerSettings` for more stuff
  * Find an alternative to the `ConfigureRun` method?
* Support fixtures being in multiple categories?

## Future, more versatile
* Option to output a script that will run similar to this, or describe the run
  * Either an actual batch file, or just a custom script langauge that can be modified externally then loaded in to concord
  - This would allow complete customization of the test runs
