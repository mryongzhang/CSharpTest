package testpowermock;

import java.io.File;


public	class ClassMyTest {
	private ClassUnderTest c;
	private FatherClass f;
	
	public  ClassMyTest(String path) {
		File file = new File(path);
		c = new ClassUnderTest();
		f = new ChildClass();
		c.foo();
	
	}
	
	public boolean test()
	{
		System.out.println("hahaha");
		if (c.foo()) {
			System.out.println("true");
		}
		else {
			System.out.println("false");
		}
		
		System.out.println(f.getStr());
		return true;
	}
}
