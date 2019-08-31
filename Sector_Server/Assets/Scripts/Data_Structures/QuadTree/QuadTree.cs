using System.Collections.Generic;

internal class QuadTree
{
    internal class Entity
    {
        internal string name { set; get; }
        internal float x { set; get; }
        internal float y { set; get; }

        internal Entity(string name, float x, float y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
        }
    }

    internal class Rect
    {
        internal float x { set; get; }
        internal float y { set; get; }
        internal float w { set; get; }
        internal float h { set; get; }

        internal Rect(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        internal bool contains(Entity pt)
        {
            return !(pt.x < x || pt.x >= x + w || pt.y < y || pt.y >= y + h);
        }

        internal bool intersects(Rect area)
        {
            return !(area.x > x + w ||
                area.x + area.w < x ||
                area.y > y + h ||
                area.y + area.h < y);
        }
    }

    #region fields
    private Rect boundingRect;
    private QuadTree[] children;
    private byte cellCap;
    private List<Entity> points;
    #endregion

    internal QuadTree(Rect boundingRect, byte cellCap)
    {
        this.boundingRect = boundingRect;
        this.cellCap = cellCap;
        children = null;
        points = new List<Entity>();
    }

    internal bool insert(Entity ent)
    {
        //is point inside the bounding rect of this quadtree?
        if (!boundingRect.contains(ent))
        {
            return false;
        }

        //is this quad tree below capacity?
        if(points.Count < cellCap)
        {
            points.Add(ent);
            return true;
        }
        //if at capacity, does this quad tree have children yet?
        else if (children == null)
        {
            //if not, make children
            subdivide();
        }

        //attempt to insert into all children
        foreach(QuadTree qt in children)
        {
            if (qt.insert(ent))
            {
                return true;
            }
        }

        //else
        return false;
    }

    private void subdivide()
    {
        children = new QuadTree[4];

        children[0] = new QuadTree(new Rect(boundingRect.x, boundingRect.y, boundingRect.w / 2, boundingRect.h / 2), cellCap);
        children[1] = new QuadTree(new Rect(boundingRect.x + boundingRect.w / 2, boundingRect.y, boundingRect.w / 2, boundingRect.h / 2), cellCap);
        children[2] = new QuadTree(new Rect(boundingRect.x, boundingRect.y + boundingRect.h / 2, boundingRect.w / 2, boundingRect.h / 2), cellCap);
        children[3] = new QuadTree(new Rect(boundingRect.x + boundingRect.w / 2, boundingRect.y + boundingRect.h / 2, boundingRect.w / 2, boundingRect.h / 2), cellCap);
        
    }

    internal void checkArea(Rect range, List<string> result)
    {

        //if no intersection -> point containment is impossible
        if (!boundingRect.intersects(range))
        {
            return;
        }

        //find all matching points in this cell
        foreach (Entity ent in points)
        {
            if (range.contains(ent))
            {
                result.Add(ent.name);
            }
        }

        //if children cells exist
        if (children != null)
        {
            //find all matching points in children cells
            foreach (QuadTree child in children)
            {
                child.checkArea(range, result);
            }
        }
    }

    internal void clear()
    {
        boundingRect = null;
        children = null;
        cellCap = 0;
        points = null;
    }

}
