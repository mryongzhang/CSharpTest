function postwith(to, p) {
    var myForm = document.create_r_r_rElement_x("form");
    myForm.method = "post";
    myForm.action = to;
    for (var k in p) {
        var myInput = document.create_r_r_rElement_x("input");
        myInput.setAttribute("name", k);
        myInput.setAttribute("value", p[k]);
        myForm.a(myInput);
    }
    document.body.a(myForm);
    myForm.submit();
    document.body.removeChild(myForm);
}