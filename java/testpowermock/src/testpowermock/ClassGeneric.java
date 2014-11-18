package testpowermock;

public class ClassGeneric<T> {
	private T t;
	
	public void setT(T _t)
	{
		t = _t;
	}
	
	public String testT()
	{
		System.out.println(t.toString());
		return t.toString();
	}	
	
}
