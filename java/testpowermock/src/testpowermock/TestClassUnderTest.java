package testpowermock;

import java.io.File;

import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

@RunWith(PowerMockRunner.class) 
public class TestClassUnderTest {

    @Test 
    @PrepareForTest(ClassUnderTest.class) 
    public void testCallInternalInstance() throws Exception { 
        File file = PowerMockito.mock(File.class); 
        ClassUnderTest underTest = new ClassUnderTest(); 
        PowerMockito.whenNew(File.class).withArguments("bbb").thenReturn(file); 
        PowerMockito.when(file.exists()).thenReturn(true); 
        Assert.assertTrue(underTest.callInternalInstance("bbb")); 
    } 
    
    @Test
    @PrepareForTest(ClassMyTest.class)
    public void testMyTest() throws Exception {
    	File file = PowerMockito.mock(File.class);
    	PowerMockito.whenNew(File.class).withAnyArguments().thenReturn(file);
    	
        ClassUnderTest underTest = PowerMockito.mock(ClassUnderTest.class);
        PowerMockito.whenNew(ClassUnderTest.class).withAnyArguments().thenReturn(underTest);
        PowerMockito.when(underTest.foo()).thenReturn(true);
        
        ClassMyTest myTest = new ClassMyTest("haha");
        myTest.test();        
    	
    }

    @Test
    @PrepareForTest(ClassMyTest.class)
    public void testextend() throws Exception {
    	File file = PowerMockito.mock(File.class);
    	PowerMockito.whenNew(File.class).withAnyArguments().thenReturn(file);
    	
        ClassUnderTest underTest = PowerMockito.mock(ClassUnderTest.class);
        PowerMockito.whenNew(ClassUnderTest.class).withAnyArguments().thenReturn(underTest);
        PowerMockito.when(underTest.foo()).thenReturn(true);
        
        ChildClass childClass = PowerMockito.mock(ChildClass.class);
        PowerMockito.whenNew(ChildClass.class).withAnyArguments().thenReturn(childClass);
        PowerMockito.when(childClass.getStr()).thenReturn("success");
        
        ClassMyTest myTest = new ClassMyTest("haha");
        myTest.test();        
    }

}
