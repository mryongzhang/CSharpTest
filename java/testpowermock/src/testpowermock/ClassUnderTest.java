package testpowermock;

import java.io.File;

//import org.mockito.runners.ConsoleSpammingMockitoJUnitRunner;

public class ClassUnderTest {	
	public boolean callInternalInstance(String path){
		File file = new File(path);
		return file.exists();	
	}
	
	public boolean foo()
	{
		return true;
	}
}

class FatherClass {
	public void f()
	{
		System.out.println("father");
	}
	
	public String getStr()
	{
		return "father";
	}
}

class ChildClass extends FatherClass {
	public void f()
	{
		System.out.println("child");
	}
	
	public String getStr()
	{
		return "child";
	}
}