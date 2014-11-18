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
	
	public boolean callPrivateMethod() {  
		try {
			isExist();
		} catch (Exception e) {
			// TODO: handle exception
			System.out.println(e.toString());
		}
		return true;  
	}	 
	 
	private boolean isExist() throws Exception{ 
		System.out.println("isExist is running!");
		return false;  
	}
}

class FatherClass {
	public void f()
	{
		System.out.println("father");
		try {
			call("ddfd");
		} catch (Exception e) {
			// TODO: handle exception
			System.out.println(e.toString());
		}
	}
	
	public String getStr()
	{
		return "father";
	}
	
	public void call(String str) throws Exception{
		throw new Exception("exception");
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