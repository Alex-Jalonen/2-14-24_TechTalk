Do you ever take issue with the noisiness of files that yell "README!!!"?

Anyways, this project is intended to be an introductory exploration of 
UniTask for devs familiar with await/async framework and a quick dive
into the Unity Jobs system.

To start the UniTask content open the `TaskCompare` scene.
- Start out by enabling the SimilarityExample game object and checking out the script by the same name.
- Next you can compare the performance of Task vs UniTask by checking out the Worker Example game object in the same scene
	- Start the scene and open the profiler
	- Enter a task cound to a worker and enable it to see the effects that it has on the CPU performance
	- Task count will only update during OnEnable, so disable and re-enable to see changes reflected
	- Notice how UniTaskWorker is nearly 100x more performant than the other two!

