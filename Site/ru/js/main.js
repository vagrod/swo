(function ($) {
    "use strict";

    /*-- Document-Ready-Function --*/
    $(document).on('ready', function () {

        /*-- Scroll-To-Top --*/
        $.scrollUp({
            scrollText: '<i class="icofont-scroll-long-up"></i>',
            easingType: 'linear',
            scrollSpeed: 900,
            animation: 'fade'
        });

        $('.single-image-slide').slick({
            infinite: true,
            slidesToShow: 1,
            slidesToScroll: 1,
            dots: true,
            arrows: false
        });

        $('.big-screen-slider').slick({
            infinite: true,
            slidesToShow: 1,
            slidesToScroll: 1,
            dots: false,
            arrows: true,
            responsive: [
                {
                    breakpoint: 1170,
                    settings: {
                        dots: true,
                        arrows: false,
                    }
                },
                {
                    breakpoint: 1024,
                    settings: {
                        dots: true,
                        arrows: false,                        
                    }
                }
            ]
        });

        $('.team-slider').slick({
            dots: true,
            arrows: false,
            infinite: true,
            speed: 1000,
            slidesToShow: 3,
            slidesToScroll: 1,
            responsive: [
                {
                    breakpoint: 1170,
                    settings: {
                        slidesToShow: 2,
                        slidesToScroll: 1,
                    }
                },
                {
                    breakpoint: 768,
                    settings: {
                        slidesToShow: 1,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        slidesToShow: 1,
                        slidesToScroll: 1
                    }
                }
            ]
        });
        $('.post-carousel').slick({
            dots: true,
            arrows: false,
            infinite: true,
            speed: 1000,
            slidesToShow: 4,
            slidesToScroll: 1,
            responsive: [
                {
                    breakpoint: 1500,
                    settings: {
                        slidesToShow: 3,
                        slidesToScroll: 1,
                    }
                },
                {
                    breakpoint: 992,
                    settings: {
                        slidesToShow: 2,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 768,
                    settings: {
                        slidesToShow: 1,
                        slidesToScroll: 1
                    }
                }
            ]
        });

        /*-- Mail-Chimp Integration--*/
        $('#mc-form').ajaxChimp({
            url: 'http://www.devitfamily.us14.list-manage.com/subscribe/post?u=b2a3f199e321346f8785d48fb&amp;id=d0323b0697', //Set Your Mailchamp URL
            callback: function (resp) {
                if (resp.result === 'success') {
                    $('.subscribe .input-box, .subscribe .bttn-4').fadeOut();
                }
            }
        });
        
        /*-- Drop-Down-Menu--*/
        function dropdown_menu() {
            $('.hamburger .mainmenu').hide();
            var sub_menu = $('.mainmenu .sub-menu'),
                menu_a = $('.mainmenu ul li a');
            sub_menu.siblings('a').append('<i class="icofont-rounded-right"></i>')
            sub_menu.hide();
            sub_menu.siblings('a').on('click', function (event) {
                event.preventDefault();
                $(this).parent('li').siblings('li').find('.sub-menu').slideUp(300);
                $(this).siblings('.sub-menu').find('.sub-menu').slideUp(300);
                $(this).siblings('.sub-menu').slideToggle(300);
                $(this).parents('li').siblings('li').removeClass('open');
                $(this).siblings('.sub-menu').find('li.open').removeClass('open');
                $(this).parent('li').toggleClass('open');
                return false;
            });
            var outside = $('body:not(.mainmenu-area ul li a)');
            outside.on('click', function () {
                $('.mainmenu-area ul .sub-menu').slideUp(300);
                $('.mainmenu-area ul li').removeClass('open');
            });
        }
        dropdown_menu();
        /*-- Burger-Menu --*/
        function burger_menu() {
            var burger = $('.burger'),
                hm_menu = $('.hamburger .mainmenu'),
                menu_item = $('.mainmenu ul li a');
            burger.on('click', function () {
                $(this).toggleClass('play');
                $(this).siblings('.mainmenu').fadeToggle();
            });
            var wSize = $(window).width();
            if(wSize <= 768){
                menu_item.on('click', function () {
                    burger.removeClass('play');
                    burger.siblings('.mainmenu').hide();
                });
            }
            $(window).on('resize',function(){
                var wSize = $(window).width();
                if(wSize < 768){                    
                    burger.removeClass('play');
                    burger.siblings('.mainmenu').hide();
                   menu_item.on('click', function () {
                        burger.removeClass('play');
                        burger.siblings('.mainmenu').hide();
                    });
                }else{
                    burger.addClass('play');
                    burger.siblings('.mainmenu').show();
                    menu_item.on('click', function () {
                        burger.siblings('.mainmenu').show();
                    });
                }
            });
        }
        burger_menu();
    });



    /*-- Window-Load-Function --*/
    $(window).on("load", function () {

        /*-- Hide-Preloader --*/
        $('.preloader').fadeOut(500);

        /*-- WoW-Animation-JS --*/
        new WOW().init({
            mobile: true,
        });

        $(".js-videoPoster").on('click', function (ev) {
            ev.preventDefault();
            var $poster = $(this);
            var $wrapper = $poster.closest('.js-videoWrapper');
            videoPlay($wrapper)
        });

        function videoPlay($wrapper) {
            var $iframe = $wrapper.find('.js-videoIframe');
            var src = $iframe.data('src');
            $wrapper.addClass('videoWrapperActive');
            $iframe.attr('src', src);
            $(".play-btn").html('<i class="icofont-ui-pause"></i>');
        }
        
        $(".play-btn").on("click", function (ev) {
            var $wrapper = $('.js-videoWrapper');
            var $iframe = $wrapper.find('.js-videoIframe');
            var src = $iframe.data('src');
            if ($wrapper.hasClass('videoWrapperActive')) {
                $wrapper.removeClass('videoWrapperActive');
                $iframe.attr('src', '');
                $(".play-btn").html('<i class="icofont-play"></i>');
            } else {
                $wrapper.addClass('videoWrapperActive');
                $iframe.attr('src', src)
                $(".play-btn").html('<i class="icofont-ui-pause"></i>');
            }
            return !1
        });

        // Select all links with hashes
        $('.mainmenu-area a[href*="#"]')
          // Remove links that don't actually link to anything
          .not('[href="#"]')
          .not('[href="#0"]')
          .on('click', function(event) {
            // On-page links
            if (
              location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') 
              && 
              location.hostname == this.hostname
            ) {
              // Figure out element to scroll to
              var target = $(this.hash);
              target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
              // Does a scroll target exist?
              if (target.length) {
                // Only prevent default if animation is actually gonna happen
                event.preventDefault();
                $('html, body').animate({
                  scrollTop: target.offset().top
                }, 1000, function() {
                  // Callback after animation
                  // Must change focus!
                  var $target = $(target);
                  $target.focus();
                  if ($target.is(":focus")) { // Checking if the target was focused
                    return false;
                  } else {
                    $target.attr('tabindex','-1'); // Adding tabindex for elements not focusable
                    $target.focus(); // Set focus again
                  };
                });
              }
            }
          });
        });
}(jQuery));