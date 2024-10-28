
function CollectionScript() {
    const collectionFilters = document.querySelector('.collection-cards-filters');

    const switch1 = document.getElementById('switch1');
    const switch2 = document.getElementById('switch2');
    const switch3 = document.getElementById('switch3');
    let pageWidth = window.innerWidth;

    function updateCardCounts(cardsCount, collectionCount) {
        document.documentElement.style.setProperty('--nft-cards-count', cardsCount);
        document.documentElement.style.setProperty('--collection-cards-count', collectionCount);
    }

    function handleSwitchChange() {
        pageWidth = window.innerWidth;
        if (switch1 && switch1.checked) {
            if (pageWidth > 1200) {
                updateCardCounts(3, 3);
            } else if (pageWidth > 992) {
                updateCardCounts(2, 2);
            }
            collectionFilters.classList.remove('layout-2');
            collectionFilters.classList.remove('layout-3');
        } else if (switch2 && switch2.checked) {
            if (pageWidth > 1200) {
                updateCardCounts(4, 3);
            } else if (pageWidth > 992) {
                updateCardCounts(3, 2);
            }
            collectionFilters.classList.add('layout-2');
            collectionFilters.classList.remove('layout-3');
        } else if (switch3 && switch3.checked) {
            if (pageWidth > 1200) {
                updateCardCounts(4, 4);
            } else if (pageWidth > 992) {
                updateCardCounts(3, 3);
            }
            collectionFilters.classList.remove('layout-2');
            collectionFilters.classList.add('layout-3');
        }
    }

    if (switch1) {
        switch1.addEventListener('change', handleSwitchChange);
        if (pageWidth < 992) {
            switch1.parentElement.classList.add('hidden');
        }
    }

    if (switch2) {
        switch2.addEventListener('change', handleSwitchChange);
    }

    if (switch3) {
        ;
        switch3.addEventListener('change', handleSwitchChange);
        if (pageWidth < 992) {
            switch3.checked = true;
        }
    }

    handleSwitchChange();
}

function ThemeScript() {
    const root = document.documentElement;
    const toggleButton = document.getElementById('theme-toggle');
    if (toggleButton) {
        const toggleButtonIcon = toggleButton.querySelector('i');
        toggleButton.addEventListener('click', () => {
            const currentTheme = root.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            root.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            toggleButtonIcon.classList.toggle('fa-moon');
            toggleButtonIcon.classList.toggle('fa-circle-half-stroke');
        });
    }

    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        root.setAttribute('data-theme', savedTheme);

        if (toggleButton) {
            const toggleButtonIcon = toggleButton.querySelector('i');
            toggleButtonIcon.classList.toggle('fa-moon');
            toggleButtonIcon.classList.toggle('fa-circle-half-stroke');
        }
    }
}

function NavMenuScript() {
    const navMenuButton = document.getElementById("nav-menu-icon");
    const navMenuIcons = navMenuButton.querySelectorAll('hr');
    const navMenu = document.getElementById("nav-menu");

    navMenuButton.addEventListener('click', () => {
        navMenu.classList.toggle('active');
        let isActive = navMenu.classList.contains('active');
        navMenu.style.height = isActive ? navMenu.scrollHeight + "px" : "0px";
        navMenuIcons.forEach((hr, key) => hr.classList.toggle(`rotated-hr${key + 1}`));
    });
}

function DropdownScript() {
    document.querySelectorAll('.dropdown').forEach(dropdown => {
        const dropdownContent = dropdown.querySelector('.dropdown-content');
        const dropdownButton = dropdown.querySelector('.dropdown-btn');
        const dropdownIcon = dropdownButton.querySelector('.arrow-icon');

        let toggle = !dropdownContent.classList.contains('opened');
        dropdownIcon.style.transform = `rotate(${toggle ? 0 : 180}deg)`;
        dropdownContent.style.height = toggle ? "0px" : dropdownContent.scrollHeight + "px";

        dropdownButton.addEventListener('click', () => {
            toggle = dropdownContent.style.height !== "0px";
            dropdownIcon.style.transform = `rotate(${toggle ? 0 : 180}deg)`;
            dropdownContent.style.height = toggle ? "0px" : dropdownContent.scrollHeight + "px";
        });
    });
}

function AutoScrollScript() {
    document.querySelectorAll('.auto-scroll').forEach((scrollbar, key) => {
        let maxScrollWidth = scrollbar.scrollWidth - scrollbar.clientWidth;
        scrollbar.scrollLeft = key % 2 == 0 ? 0 : maxScrollWidth;
        let direction = 1;
        let pause = false;

        setInterval(() => {
            if (!pause) {
                scrollbar.scrollBy(direction, 0);

                if (scrollbar.scrollLeft >= maxScrollWidth) {
                    direction = -1;
                    pause = true;
                    setTimeout(() => { pause = false; }, 1000);
                }
                else if (scrollbar.scrollLeft <= 0) {
                    direction = 1;
                    pause = true;
                    setTimeout(() => { pause = false; }, 1000);
                }
            }
        }, 30)
    });
}
