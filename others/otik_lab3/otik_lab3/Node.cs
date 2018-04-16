

namespace otik_lab3
{
    public class Node
    {
        public byte sym;
        public Node left;
        public Node right;
        public static byte B=0;

        public void build(string str,byte b)
        {
            if (str == "")
            {
                sym = b;
                return;
            }
            if (str[0] == '0')
            {
                if (this.left == null)
                {
                    var newnode = new Node();
                    this.left = newnode;
                    newnode.build(str.Substring(1),b);
                }
                else
                {
                    this.left.build(str.Substring(1),b);
                }
            }
            else if (str[0] == '1')
            {
                if (this.right == null)
                {
                    var newnode = new Node();
                    this.right = newnode;
                    newnode.build(str.Substring(1),b);
                }
                else
                {
                    this.right.build(str.Substring(1),b);
                }
            }
        }

        public bool search(string s)
        {
            if (s == "" && this.left == null && this.right == null)
            {
                B = sym;
                return true;
            }
            else if (s == "")
                return false;
            if (s[0] == '0')
            {
                if (this.left != null)
                    return this.left.search(s.Substring(1));
            }
            else if (s[0] == '1')
            {
                if (this.right != null)
                    return this.right.search(s.Substring(1));
            }
            return false;
        }
    }
}
