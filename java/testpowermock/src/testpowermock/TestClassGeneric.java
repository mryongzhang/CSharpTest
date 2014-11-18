package testpowermock;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mock;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

@RunWith(PowerMockRunner.class) 
public class TestClassGeneric {
	
	//方法1：使用强制转换的方法
    @Test 
    @SuppressWarnings("unchecked") 
    @PrepareForTest(ClassGeneric.class) 
	public void testGeneric1() {
		ClassGeneric<Integer> cg = (ClassGeneric<Integer>)PowerMockito.mock(ClassGeneric.class);		
		PowerMockito.when(cg.testT()).thenReturn("testGeneric1");		
		System.out.println(cg.testT());			
	}
    
    //方法2：使用@Mock标注，实现声明出一个对象，然后在测试函数中直接使用
	@Mock
	public ClassGeneric<Integer> intMock;
	
    @Test 
    @PrepareForTest(ClassGeneric.class) 
	public void testGeneric2() {	
		PowerMockito.when(intMock.testT()).thenReturn("testGeneric2");		
		System.out.println(intMock.testT());			
	}
    
    //方法3：通过继承泛型类的方法，先创建出一个中间类，之后mock这个中间类
    public class IntGeneric extends ClassGeneric<Integer>
    {    		
    }
    
    @Test 
    @PrepareForTest(ClassGeneric.class) 
    public void testGeneric3() {	
    	ClassGeneric<Integer> cg = PowerMockito.mock(IntGeneric.class);
    	PowerMockito.when(cg.testT()).thenReturn("testGeneric3");		
		System.out.println(cg.testT());			
    	
    }
}
