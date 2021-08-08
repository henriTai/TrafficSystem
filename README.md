# TrafficSystem

Traffic System for Unity

 ABOUT THIS PROJECT

This traffic system during a 3 month period in 2019 for a Helsinki XR Center's VR ambulance project. The goal was to enable realistic traffic simulation in traffic environment based on real-life locations.

More information about this traffic system from these external links:

Google Drive: https://drive.google.com/drive/folders/1Mq-2g1W1R1YxviLj1v5VBx3vKZkOwcDn?usp=sharing
* User guide .doc (in English)
* Traffic demonstration video (in Finnish)
* User guide video for tools (in Finnish)

I also discussed the traffic system in detail in my thesis (in Finnish): http://urn.fi/URN:NBN:fi:amk-2020093020814

Doxygen documentation of the traffic system: https://www.waypoint.htainio.fi/

AUTHOR

I developed the traffic system by myself, although I took pointers from various sources (Bezier splines related math, traffic control protocols for autonomous vehicles).

PROJECT AS A LEARNING EXPERIENCE

Considering the short time frame and lack of previous experience of developing such a system, the learning curve was quite steep. From that standpoint I'm pleased of what I was able to achieve during the project (although there are flaws that I'm aware and many features had to be left out for possible furher development). Besides various aspects of traffic and traffic simulation, I learned a lot about creating editor extension tools and thinking about usability.

SKILLSET DEMONSTRATION

I hope this sample showcases my general know-how with Unity at at this moment and my ability to design tools to enhance workflow. Of these scripts I'm especially pleased with the road creation tool scripts (ParallelBezierSplines.cs, ParallelBezierSplinesInspector.cs, IntersectionTool.cs, IntersectionToolInspector.cs). The usage of these tools is demonstrated in the this video https://drive.google.com/file/d/1eotpLKeEh9e24v911EbFzfwRBqIF4iVL/view?usp=sharing and this written user guide https://drive.google.com/file/d/1jwPRdeip8V-d_bzuRwhruFJoF9e0enQS/view?usp=sharing (both in finnish).

ABOUT THE TRAFFIC SYSTEM

The traffic system is developed for Unity. It consists of 3 main areas + 1 separate tool:

1) Road network / node network

All the information about routes, traffic rules and limitations is stored in this hierarcal data structure. Hierarchical structure also enables quick search of route to any destination and AI interaction between agents (duty to yield). On the base level, the routes are constructed from linked nodes. One level up in the hierarchy, the nodes are parented to a lane object and on the next level of the hierarchy, the lanes are parented to a road object. Road objects can be either intersections or straight roads. On the top of the hierarchy, all road objects are gathered under a road network object.

2) Tool scripts

This is the part of the project I spent most time working on. In development I tried to balance the ease of use and versatility. With these tools, it is relatively easy and quick to design a road network, configure intersections and set traffic lights systems and crosswalks. Tool scripts are quite large and they generate their own data objects, which are indenpendent from the actual road network. The idea is that tools and their data objects can be removed from the final build.

3) AI Scripts

AI scripts are divided to two sections: a) Vehicle AI scripts form AI agent (car) behaviour and b) traffic AI scripts consist of intersection and controllers that AI agents communicate with. Traffic controllers coordinate traffic in critical areas such as intersections freeing the AI agents from unnesseccary evaluations.

4) Route Utility

Route Utility is a separate extension to the traffic system. It can be used for generating a navigation database in editor. This database stores shortest routes from each location on the road network to any other location and can be utilized for quick navigation.
