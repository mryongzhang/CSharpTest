package testpowermock;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mock;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

@RunWith(PowerMockRunner.class) 
public class TestClassGeneric {
	
	//����1��ʹ��ǿ��ת���ķ���
    @Test 
    @SuppressWarnings("unchecked") 
    @PrepareForTest(ClassGeneric.class) 
	public void testGeneric1() {
		ClassGeneric<Integer> cg = (ClassGeneric<Integer>)PowerMockito.mock(ClassGeneric.class);		
		PowerMockito.when(cg.testT()).thenReturn("testGeneric1");		
		System.out.println(cg.testT());			
	}
    
    //����2��ʹ��@Mock��ע��ʵ��������һ������Ȼ���ڲ��Ժ�����ֱ��ʹ��
	@Mock
	public ClassGeneric<Integer> intMock;
	
    @Test 
    @PrepareForTest(ClassGeneric.class) 
	public void testGeneric2() {	
		PowerMockito.when(intMock.testT()).thenReturn("testGeneric2");		
		System.out.println(intMock.testT());			
	}
    
    //����3��ͨ���̳з�����ķ������ȴ�����һ���м��֮࣬��mock����м���
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
