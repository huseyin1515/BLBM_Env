$(document).ready(function () {
    $("#menu-toggle").click(function (e) {
        e.preventDefault();
        $("#wrapper").toggleClass("toggled");
    });
    $(function () {
        var currentPath = location.pathname;
        if (currentPath === '/') { currentPath = '/Envanter/Index'; }
        $('.list-group-item').each(function () {
            var $this = $(this);
            if (currentPath.toLowerCase().startsWith($this.attr('href').toLowerCase())) {
                $('.list-group-item').removeClass('active');
                $this.addClass('active');
            }
        });
    });
});