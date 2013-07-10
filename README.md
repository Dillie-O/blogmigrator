blogmigrator
============

The Blog Migrator tool is an all purpose utility designed to help transition a blog from one platform to another. It leverages XML-RPC, BlogML, and WordPress WXR formats. It also provides the ability to "rewrite" your posts on your old blog to point to the new location.

The Blog Migrator started out as a bounty by Roy Osherove (in 2010) looking for a way to migrate out from the weblogs.asp.net site into SquareSpace. By default, SquareSpace did not have an automatic import tool for BlogML generated export files. Thus this project was born.

Blog Migrator can handle both simple (only post) migrations and complete (posts, comments, etc.) depending on which format you use. Blog Migrator can read BlogML dumped files or connect directly to your site via XML-RPC. You can publish to your new site via XML-RPC, or generate a WordPress WXR document, which is widely used by numerous blog services to import content. One of the most handy features of Blog Migrator is the ability to update your "source" blog posts to point to the destination blog, thus helping your users know of your new location.

While functional, there are lots of additional features that can be added (Movable Type/Blogger format support, previews of source/destination content). You are encouraged to help make this tool even better by taking the source, looking at features/bugs, and improve things even more!

This code was copied over from my codeplex repository so that those that want to contribute have an easier way of doing so.
